﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;



namespace MyBudget.Core.Tools
{
    public class PdfParser
    {
        // G:\Solutions\my-budget\report.pdf
        public void Parse(string path)
        {
            using (var reader = new PdfReader(path))
            {
                using (var pdfDocument = new PdfDocument(reader))
                {
                    //var strategy = new LocationTextExtractionStrategy();
                    //StringBuilder processed = new StringBuilder();

                    //PdfDocumentContentParser parser = new PdfDocumentContentParser(pdfDocument);

                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                       // var t = parser.ProcessContent(i, new LocationTextExtractionStrategy());

                       // Console.WriteLine(t);

                        var page = pdfDocument.GetPage(i);

                        var a1 = page.GetContentBytes();
                        var a2 = Encoding.Convert(Encoding.Default, Encoding.UTF8, a1);

                        string textFromPage = Encoding.UTF8.GetString(
                            Encoding.Convert(
                                Encoding.Default, Encoding.UTF8, page.GetContentBytes()));

                        var text2 = GetDataConvertedData(textFromPage);

                        //  string text = PdfTextExtractor.GetTextFromPage(page);

                        // processed.Append(text);
                        Console.WriteLine(text2);
                    }
                }
            }
        }

        //public void Parse(string path)
        //{
        //    var doc = PdfReader.Open(path, PdfDocumentOpenMode.Import);


        //    var pages = new List<String>();

        //    for (int i = 0; i < doc.PageCount; i++)
        //    {
        //        string textFromPage = Encoding.UTF8.GetString(
        //            Encoding.Convert(
        //                Encoding.Default, Encoding.UTF8, doc.Pages[i].Stream.Value));

        //        pages.Add(GetDataConvertedData(textFromPage));
        //    }

        //    Console.WriteLine(pages);
        //}

        string GetDataConvertedData(string textFromPage)
        {
            var texts = textFromPage.Split(new[] { "\n" }, StringSplitOptions.None)
                                    .Where(text => text.Contains("Tj")).ToList();

            return texts.Aggregate(string.Empty, (current, t) => current +
                       t.TrimStart('(')
                        .TrimEnd('j')
                        .TrimEnd('T')
                        .TrimEnd(')'));
        }
    }
}

public class PDFParser
{
    /// BT = Beginning of a text object operator 
    /// ET = End of a text object operator
    /// Td move to the start of next line
    ///  5 Ts = superscript
    /// -5 Ts = subscript

    #region Fields

    #region _numberOfCharsToKeep
    /// <summary>
    /// The number of characters to keep, when extracting text.
    /// </summary>
    private static int _numberOfCharsToKeep = 15;
    #endregion

    #endregion

    #region ExtractText
    /// <summary>
    /// Extracts a text from a PDF file.
    /// </summary>
    /// <param name="inFileName">the full path to the pdf file.</param>
    /// <param name="outFileName">the output file name.</param>
    /// <returns>the extracted text</returns>
    public bool ExtractText(string inFileName, string outFileName)
    {
        StreamWriter outFile = null;
        try
        {
            // Create a reader for the given PDF file
            PdfReader reader = new PdfReader(inFileName);
            //outFile = File.CreateText(outFileName);
            outFile = new StreamWriter(outFileName, false, System.Text.Encoding.UTF8);

            Console.Write("Processing: ");

            var pdfDocument = new PdfDocument(reader);

            int totalLen = 68;
            float charUnit = ((float)totalLen) / (float)pdfDocument.GetNumberOfPages();
            int totalWritten = 0;
            float curUnit = 0;

            for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                var pag2e = pdfDocument.GetPage(page);

                pag2e.GetContentBytes();

                outFile.Write(ExtractTextFromPDFBytes(pag2e.GetContentBytes()) + " ");

                // Write the progress.
                if (charUnit >= 1.0f)
                {
                    for (int i = 0; i < (int)charUnit; i++)
                    {
                        Console.Write("#");
                        totalWritten++;
                    }
                }
                else
                {
                    curUnit += charUnit;
                    if (curUnit >= 1.0f)
                    {
                        for (int i = 0; i < (int)curUnit; i++)
                        {
                            Console.Write("#");
                            totalWritten++;
                        }
                        curUnit = 0;
                    }

                }
            }

            if (totalWritten < totalLen)
            {
                for (int i = 0; i < (totalLen - totalWritten); i++)
                {
                    Console.Write("#");
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (outFile != null) outFile.Close();
        }
    }
    #endregion

    #region ExtractTextFromPDFBytes
    /// <summary>
    /// This method processes an uncompressed Adobe (text) object 
    /// and extracts text.
    /// </summary>
    /// <param name="input">uncompressed</param>
    /// <returns></returns>
    public string ExtractTextFromPDFBytes(byte[] input)
    {
        if (input == null || input.Length == 0) return "";

        try
        {
            string resultString = "";

            // Flag showing if we are we currently inside a text object
            bool inTextObject = false;

            // Flag showing if the next character is literal 
            // e.g. '\\' to get a '\' character or '\(' to get '('
            bool nextLiteral = false;

            // () Bracket nesting level. Text appears inside ()
            int bracketDepth = 0;

            // Keep previous chars to get extract numbers etc.:
            char[] previousCharacters = new char[_numberOfCharsToKeep];
            for (int j = 0; j < _numberOfCharsToKeep; j++) previousCharacters[j] = ' ';


            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)input[i];
                if (input[i] == 213)
                    c = "'".ToCharArray()[0];

                if (inTextObject)
                {
                    // Position the text
                    if (bracketDepth == 0)
                    {
                        if (CheckToken(new string[] { "TD", "Td" }, previousCharacters))
                        {
                            resultString += "\n\r";
                        }
                        else
                        {
                            if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters))
                            {
                                resultString += "\n";
                            }
                            else
                            {
                                if (CheckToken(new string[] { "Tj" }, previousCharacters))
                                {
                                    resultString += " ";
                                }
                            }
                        }
                    }

                    // End of a text object, also go to a new line.
                    if (bracketDepth == 0 &&
                        CheckToken(new string[] { "ET" }, previousCharacters))
                    {

                        inTextObject = false;
                        resultString += " ";
                    }
                    else
                    {
                        // Start outputting text
                        if ((c == '(') && (bracketDepth == 0) && (!nextLiteral))
                        {
                            bracketDepth = 1;
                        }
                        else
                        {
                            // Stop outputting text
                            if ((c == ')') && (bracketDepth == 1) && (!nextLiteral))
                            {
                                bracketDepth = 0;
                            }
                            else
                            {
                                // Just a normal text character:
                                if (bracketDepth == 1)
                                {
                                    // Only print out next character no matter what. 
                                    // Do not interpret.
                                    if (c == '\\' && !nextLiteral)
                                    {
                                        resultString += c.ToString();
                                        nextLiteral = true;
                                    }
                                    else
                                    {
                                        if (((c >= ' ') && (c <= '~')) ||
                                            ((c >= 128) && (c < 255)))
                                        {
                                            resultString += c.ToString();
                                        }

                                        nextLiteral = false;
                                    }
                                }
                            }
                        }
                    }
                }

                // Store the recent characters for 
                // when we have to go back for a checking
                for (int j = 0; j < _numberOfCharsToKeep - 1; j++)
                {
                    previousCharacters[j] = previousCharacters[j + 1];
                }
                previousCharacters[_numberOfCharsToKeep - 1] = c;

                // Start of a text object
                if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters))
                {
                    inTextObject = true;
                }
            }

            return CleanupContent(resultString);
        }
        catch
        {
            return "";
        }
    }

    private string CleanupContent(string text)
    {
        string[] patterns = { @"\\\(", @"\\\)", @"\\226", @"\\222", @"\\223", @"\\224", @"\\340", @"\\342", @"\\344", @"\\300", @"\\302", @"\\304", @"\\351", @"\\350", @"\\352", @"\\353", @"\\311", @"\\310", @"\\312", @"\\313", @"\\362", @"\\364", @"\\366", @"\\322", @"\\324", @"\\326", @"\\354", @"\\356", @"\\357", @"\\314", @"\\316", @"\\317", @"\\347", @"\\307", @"\\371", @"\\373", @"\\374", @"\\331", @"\\333", @"\\334", @"\\256", @"\\231", @"\\253", @"\\273", @"\\251", @"\\221" };
        string[] replace = { "(", ")", "-", "'", "\"", "\"", "à", "â", "ä", "À", "Â", "Ä", "é", "è", "ê", "ë", "É", "È", "Ê", "Ë", "ò", "ô", "ö", "Ò", "Ô", "Ö", "ì", "î", "ï", "Ì", "Î", "Ï", "ç", "Ç", "ù", "û", "ü", "Ù", "Û", "Ü", "®", "™", "«", "»", "©", "'" };

        for (int i = 0; i < patterns.Length; i++)
        {
            string regExPattern = patterns[i];
            Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
            text = regex.Replace(text, replace[i]);
        }

        return text;
    }

    #endregion

    #region CheckToken
    /// <summary>
    /// Check if a certain 2 character token just came along (e.g. BT)
    /// </summary>
    /// <param name="tokens">the searched token</param>
    /// <param name="recent">the recent character array</param>
    /// <returns></returns>
    private bool CheckToken(string[] tokens, char[] recent)
    {
        try
        {
            foreach (string token in tokens)
            {
                if ((recent[_numberOfCharsToKeep - 3] == token[0]) &&
                    (recent[_numberOfCharsToKeep - 2] == token[1]) &&
                    ((recent[_numberOfCharsToKeep - 1] == ' ') ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0a)) &&
                    ((recent[_numberOfCharsToKeep - 4] == ' ') ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0a))
                    )
                {
                    return true;
                }
            }
        }
        catch (Exception)
        {
            return true;
        }
        return false;
    }
    #endregion
}
