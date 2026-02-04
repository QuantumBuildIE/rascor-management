export interface Site {
  id: string;
  siteCode: string;
  siteName: string;
  address?: string;
  city?: string;
  postalCode?: string;
  siteManagerId?: string;
  siteManagerName?: string;
  companyId?: string;
  companyName?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  notes?: string;
}

export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  phone?: string;
  mobile?: string;
  jobTitle?: string;
  department?: string;
  primarySiteId?: string;
  primarySiteName?: string;
  startDate?: string;
  endDate?: string;
  isActive: boolean;
  notes?: string;
  /** Indicates whether this employee has a linked User account */
  hasUserAccount?: boolean;
  /** The linked User ID if a user account exists */
  linkedUserId?: string;
  /** GeoTracker device ID for site attendance tracking */
  geoTrackerId?: string;
  /** Preferred language for Toolbox Talk subtitles and notifications (ISO 639-1 code) */
  preferredLanguage: string;
  /** Float person ID - links this employee to a Float person record for schedule integration */
  floatPersonId?: number;
  /** When this employee was linked to Float */
  floatLinkedAt?: string;
  /** How this employee was linked to Float (Auto-Email, Auto-Name, Manual) */
  floatLinkMethod?: string;
}

export interface CreateEmployeeRequest {
  employeeCode: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  mobile?: string;
  jobTitle?: string;
  department?: string;
  primarySiteId?: string;
  startDate?: string;
  endDate?: string;
  isActive: boolean;
  notes?: string;
  /** If true, creates a linked User account when Email is provided */
  createUserAccount?: boolean;
  /** Optional role name to assign to the created user */
  userRole?: string;
  /** GeoTracker device ID for site attendance tracking */
  geoTrackerId?: string;
  /** Preferred language for Toolbox Talk subtitles and notifications (ISO 639-1 code) */
  preferredLanguage?: string;
  /** Float person ID - links this employee to a Float person record for schedule integration */
  floatPersonId?: number;
}

export interface Company {
  id: string;
  companyCode: string;
  companyName: string;
  tradingName?: string;
  phone?: string;
  email?: string;
  companyType?: string;
  isActive: boolean;
}

export interface Contact {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  jobTitle?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  companyId?: string;
  companyName?: string;
  siteId?: string;
  siteName?: string;
  isPrimaryContact: boolean;
  isActive: boolean;
  notes?: string;
}

/** Admin device information for listing view */
export interface AdminDevice {
  id: string;
  /** Device identifier string (e.g., EVT0001) */
  deviceIdentifier: string;
  /** Platform: iOS, Android */
  platform?: string;
  /** Device name or model */
  deviceName?: string;
  /** When the device was first registered */
  registeredAt: string;
  /** Last time the device was active */
  lastActiveAt?: string;
  /** Whether the device is currently active */
  isActive: boolean;
  /** Linked employee ID */
  employeeId?: string;
  /** Linked employee name */
  employeeName?: string;
  /** Linked employee email */
  employeeEmail?: string;
  /** When the device was linked */
  linkedAt?: string;
  /** Who linked the device */
  linkedBy?: string;
  /** Whether the device is linked to an employee */
  isLinked: boolean;
}

/** Admin device detail including unlinking history */
export interface AdminDeviceDetail extends AdminDevice {
  /** Push notification token */
  pushToken?: string;
  /** When the device was last unlinked */
  unlinkedAt?: string;
  /** Reason provided when device was unlinked */
  unlinkedReason?: string;
}
