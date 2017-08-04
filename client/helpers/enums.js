export const statUnitTypes = new Map([
  [1, 'LocalUnit'],
  [2, 'LegalUnit'],
  [3, 'EnterpriseUnit'],
  [4, 'EnterpriseGroup'],
])

export const statUnitIcons = new Map([
  [1, 'suitcase'],
  [2, 'briefcase'],
  [3, 'building'],
  [4, 'sitemap'],
])

export const dataSourceOperations = new Map([
  [1, 'Create'],
  [2, 'Update'],
  [3, 'CreateAndUpdate'],
])

export const dataSourcePriorities = new Map([
  [1, 'NotTrusted'],
  [2, 'Ok'],
  [3, 'Trusted'],
])

export const dataSourceQueueStatuses = new Map([
  [1, 'InQueue'],
  [2, 'Loading'],
  [3, 'DataLoadCompleted'],
  [4, 'DataLoadCompletedPartially'],
])

export const dataSourceQueueLogStatuses = new Map([
  [1, 'Done'],
  [2, 'Warning'],
  [3, 'Error'],
])

export const personSex = new Map([
  [1, 'Male'],
  [2, 'Female'],
])

export const personTypes = new Map([
  [1, 'ContactPerson'],
  [2, 'Founder'],
  [3, 'Owner'],
])

export const userStatuses = new Map([
  [0, 'Suspended'],
  [1, 'Active'],
])
