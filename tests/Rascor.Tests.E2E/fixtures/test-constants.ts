/**
 * Test constants and data identifiers for E2E tests
 * These should match the seeded test data in the database
 */
export const TEST_TENANT = {
  id: '11111111-1111-1111-1111-111111111111',

  users: {
    admin: {
      email: 'admin@rascor.ie',
      password: 'Admin123!',
      homePage: '/dashboard',
    },
    warehouse: {
      email: 'warehouse@rascor.ie',
      password: 'Warehouse123!',
      homePage: '/stock',
    },
    siteManager: {
      email: 'sitemanager@rascor.ie',
      password: 'SiteManager123!',
      homePage: '/stock/orders',
    },
    officeStaff: {
      email: 'office@rascor.ie',
      password: 'Office123!',
      homePage: '/dashboard',
    },
    finance: {
      email: 'finance@rascor.ie',
      password: 'Finance123!',
      homePage: '/dashboard',
    },
  },

  sites: {
    quantumBuild: { name: 'Quantum Build', location: 'Dublin' },
    southWestGate: { name: 'South West Gate', location: 'Cork' },
    marmaladeLane: { name: 'Marmalade Lane', location: 'Galway' },
  },

  categories: {
    buildingMaterials: 'Building Materials',
    electrical: 'Electrical',
    plumbing: 'Plumbing',
    safetyEquipment: 'Safety Equipment',
    tools: 'Tools',
    fixingsFasteners: 'Fixings & Fasteners',
  },

  stockLocations: {
    mainWarehouse: 'Main Warehouse',
  },
};

/**
 * Test data generation helpers
 */
export const generateTestData = {
  uniqueString: (prefix: string = 'test') =>
    `${prefix}_${Date.now()}_${Math.random().toString(36).substring(7)}`,

  uniqueEmail: () =>
    `test_${Date.now()}@test.rascor.ie`,

  uniqueReference: (prefix: string) =>
    `${prefix}-${Date.now().toString().slice(-6)}`,
};

/**
 * API endpoints for test verification
 */
export const API_ENDPOINTS = {
  auth: {
    login: '/api/auth/login',
    me: '/api/auth/me',
    refresh: '/api/auth/refresh-token',
  },
  stock: {
    products: '/api/products',
    categories: '/api/categories',
    orders: '/api/stock-orders',
    purchaseOrders: '/api/purchase-orders',
    goodsReceipts: '/api/goods-receipts',
    stockLevels: '/api/stock-levels',
    stocktakes: '/api/stocktakes',
  },
  proposals: {
    proposals: '/api/proposals',
    productKits: '/api/product-kits',
    reports: '/api/proposals/reports',
  },
  siteAttendance: {
    events: '/api/site-attendance/events',
    summaries: '/api/site-attendance/summaries',
    dashboard: '/api/site-attendance/dashboard',
    settings: '/api/site-attendance/settings',
    bankHolidays: '/api/site-attendance/bank-holidays',
  },
  admin: {
    users: '/api/users',
    roles: '/api/roles',
    sites: '/api/sites',
    employees: '/api/employees',
    companies: '/api/companies',
    contacts: '/api/contacts',
  },
};

/**
 * Wait times and timeouts
 */
export const TIMEOUTS = {
  short: 5000,
  medium: 10000,
  long: 30000,
  navigation: 15000,
  api: 10000,
};

/**
 * Test tags for filtering
 */
export const TAGS = {
  smoke: '@smoke',
  regression: '@regression',
  critical: '@critical',
  slow: '@slow',
  flaky: '@flaky',
};
