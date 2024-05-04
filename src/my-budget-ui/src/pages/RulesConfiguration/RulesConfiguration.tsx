import { Container } from '@mui/material';
import { FC, useState } from 'react';

import FileLoader from 'common/FileLoader';
import Section from 'common/Section';
import FlexRow from 'common/FlexRow';
import { IRule } from 'types';

import Rule from './Rule'

const RulesConfigurator: FC = () => {
  const [data, setData] = useState<IRule[]>([]);

  const onFileContentHandler = (content: string) => {
    try {
      const parsedData: IRule[] = JSON.parse(content);
      setData(parsedData);
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <Container>
      <Section>
        <FlexRow>
          <FileLoader onFileContent={onFileContentHandler} />
        </FlexRow>
      </Section>
      {data.length > 0 && (
        <Section>
          {data.map((rule, index) => <Rule key={index} rule={rule} />)}
        </Section>
      )}
    </Container>
  );
};

export default RulesConfigurator;
