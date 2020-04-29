import { omitBy } from 'lodash'
import React, { useEffect } from 'react'
import { shallowEqual, useDispatch, useSelector } from 'react-redux';

import { AssociationData } from '../../../apis/referee';
import { getNationalGoverningBodies } from '../../../modules/nationalGoverningBody/nationalGoverningBodies';
import { RootState } from '../../../rootReducer';
import { Datum } from '../../../schemas/getNationalGoverningBodiesSchema';
import { IncludedAttributes } from '../../../schemas/getRefereeSchema';

interface LocationProps {
  ngbs: IncludedAttributes[];
  locations: IncludedAttributes[];
  isEditing: boolean;
  onChange: (value: AssociationData, stateKey: string) => void;
  value: AssociationData;
}

const RefereeLocation = (props: LocationProps) => {
  const { ngbs, locations, isEditing, value, onChange } = props;
  const dispatch = useDispatch()
  const { allNgbs } = useSelector((state: RootState) => {
    return {
      allNgbs: state.nationalGoverningBodies.nationalGoverningBodies,
    }
  }, shallowEqual)

  useEffect(() => {
    if(isEditing) {
      dispatch(getNationalGoverningBodies())      
    }
  }, [isEditing])

  const hasType = (type: string): boolean => {
    return locations.filter((location) => location.associationType === type).length > 0
  };

  const getNgbName = (type: string): string => {
    const ngbId = locations.find(
      (location) => location.associationType === type
    )?.nationalGoverningBodyId;
    
    return ngbs.find((ngb) => ngb.nationalGoverningBodyId === ngbId)?.name;
  };

  const getSelectedNgb = (type: string) => {
    return locations.filter(location => location.associationType === type)[0]?.nationalGoverningBodyId
  }

  const handleChange = (type: string) => (event: React.ChangeEvent<HTMLSelectElement>) => {
    let updatedValue: AssociationData = {...value}
    const newNGB = event.target.value
    const isBlank = newNGB === '-1'
    const hasTypeInValue = Object.values(value).includes(type)
    
    if (hasTypeInValue && !isBlank) {
      const filtered = omitBy(value, (existingType: string) => existingType === type)
      updatedValue = Object.assign(filtered, { [event.target.value]: type })
    } else if(isBlank) {
      updatedValue = omitBy(value, (existingType: string) => existingType === type)
    } else {
      updatedValue[newNGB] = type
    }

    onChange(updatedValue, 'ngbData')
  }

  const renderOption = (ngb: Datum) => <option key={ngb.id} value={ngb.id}>{ngb.attributes.name}</option>
  const renderDropdown = (type: string) => {
    return (
      <select className="form-select block mt-1" onChange={handleChange(type)} value={getSelectedNgb(type)}>
        {type === 'secondary' && <option value="-1">None</option>}
        {allNgbs.map(renderOption)}
      </select>
    )
  }
  const emptyNgb = 'National Governing Body not selected'

  return (
    <div className="flex flex-col w-1/2 p-4">
      <div className="w-full mb-4">
        <h4 className="text-sm mb-2">Primary NGB</h4>
        {!isEditing && (
          <p className="font-bold">{hasType('primary') ? getNgbName("primary") : emptyNgb}</p>
        )}
        {isEditing && renderDropdown('primary')}
      </div>
      <div className="w-full">
        <h4 className="text-sm mb-2">Secondary NGB</h4>
        {!isEditing && (
          <p className="font-bold">{hasType('secondary') ? getNgbName("secondary"): emptyNgb}</p>
        )}
        {isEditing && renderDropdown('secondary')}
      </div>
    </div>
  );
}

export default RefereeLocation
