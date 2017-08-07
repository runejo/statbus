export const castEmptyOrNull = x =>
  x === ''
    ? null
    : x === null
      ? undefined
      : x

export const createModel = ({ dataAccess, properties }) =>
  Object.entries(properties)
    .reduce(
      (acc, [, v]) => ({
        ...acc,
        [v.name]: castEmptyOrNull(v.value),
      }),
      { dataAccess },
    )

export const updateProperties = (model, properties) =>
  properties.map(
    p => model[p.name] === undefined
      ? p
      : { ...p, value: model[p.name] },
  )
