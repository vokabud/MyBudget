import { Box, MenuItem, Select, SelectChangeEvent, TextField, Typography } from '@mui/material';
import { FC, useEffect, useState } from 'react';

import {
  IRule,
  Property,
  RuleCondition,
  RuleResultType,
} from 'types';

interface IProps {
  value: IRule;
}

const Rule: FC<IProps> = ({ value }) => {
  const [rule, setRule] = useState<IRule | null>(null);

  useEffect(() => {
    setRule(value);
  }, [value]);

  const handlePropertyChange = (event: SelectChangeEvent) => {
    const property = event.target.value as Property;

    if (property && rule) {
      setRule({ ...rule, property: property });
    }
  };

  const handleConditionChange = (event: SelectChangeEvent) => {
    const condition = event.target.value;
    console.log(condition);
    console.log(event.target.value);

    if (condition && rule) {
      setRule({
        ...rule,
        condition: condition == RuleCondition[RuleCondition.Contains]
          ? RuleCondition.Contains
          : RuleCondition.Equals
      });
    }
  };

  const handleValueChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value;

    if (value && rule) {
      setRule({ ...rule, value: value });
    }
  }

  const handleResultTypeChange = (event: SelectChangeEvent) => {
    const resultType = event.target.value;

    if (resultType && rule) {
      setRule({
        ...rule,
        result: {
          ...rule.result,
          type: resultType == RuleResultType[RuleResultType.FromProperty]
            ? RuleResultType.FromProperty
            : RuleResultType.FromValue
        }
      });
    }
  }

  if (rule === null) {
    return null;
  }

  return (
    <>
      <Typography>
        If property {Property[rule.property]} is {RuleCondition[rule.condition]}  {rule.value} then set {RuleResultType[rule.result.type]} to {rule.result.value} for property {Property[rule.result.property]}
      </Typography>
      <Box>
        <Select
          variant={'standard'}
          value={rule.property}
          onChange={handlePropertyChange}
        >
          <MenuItem value={Property.Currency}>Currency</MenuItem>
          <MenuItem value={Property.Details}>Details</MenuItem>
          <MenuItem value={Property.MCC}>MCC</MenuItem>
        </Select>

        <Select
          variant={'standard'}
          value={RuleCondition[rule.condition]}
          onChange={handleConditionChange}
        >
          <MenuItem value={RuleCondition[RuleCondition.Contains]}>Contains</MenuItem>
          <MenuItem value={RuleCondition[RuleCondition.Equals]}>Equals</MenuItem>
        </Select>

        <TextField
          value={rule.value}
          variant={'standard'}
          onChange={handleValueChange}
        />

        <Select
          variant={'standard'}
          value={RuleResultType[rule.result.type]}
          onChange={handleResultTypeChange}
        >
          <MenuItem value={RuleResultType[RuleResultType.FromProperty]}>to property</MenuItem>
          <MenuItem value={RuleResultType[RuleResultType.FromValue]}>to value</MenuItem>
        </Select>

      </Box>
    </>
  );
};

export default Rule;