namespace MediCareX.Domain.Enums;

public enum AppointmentStatus { Pending = 0, Approved = 1, Rejected = 2, Rescheduled = 3, InProgress = 4, Completed = 5, Cancelled = 6, NoShow = 7 }
public enum AppointmentType { Online = 0, InPerson = 1 }
public enum PaymentStatus { Pending = 0, Processing = 1, Succeeded = 2, Failed = 3, Refunded = 4, PartiallyRefunded = 5, Cancelled = 6 }
public enum PaymentMethod { Stripe = 0, JazzCash = 1, EasyPaisa = 2, PayPal = 3, Wallet = 4 }
public enum PaymentPurpose { Consultation = 0, Medicine = 1, LabTest = 2, WalletTopUp = 3, Subscription = 4 }
public enum TransactionType { Credit = 0, Debit = 1 }
public enum UserRole { Admin = 0, Doctor = 1, Patient = 2, Pharmacist = 3, LabTechnician = 4, Receptionist = 5 }
public enum BloodGroup { Unknown = 0, APositive = 1, ANegative = 2, BPositive = 3, BNegative = 4, ABPositive = 5, ABNegative = 6, OPositive = 7, ONegative = 8 }
public enum Gender { Unspecified = 0, Male = 1, Female = 2, Other = 3 }
public enum NotificationType { General = 0, Appointment = 1, Prescription = 2, Payment = 3, LabReport = 4, MedicineReminder = 5, Emergency = 6, System = 7, Chat = 8 }
public enum LabTestStatus { Booked = 0, SampleScheduled = 1, SampleCollected = 2, Processing = 3, Completed = 4, Cancelled = 5 }
public enum DiseaseSeverity { Mild = 0, Moderate = 1, Severe = 2, Critical = 3 }
public enum RecordType { Diagnosis = 0, LabReport = 1, Prescription = 2, Imaging = 3, Vaccination = 4, Surgery = 5, Other = 6 }
public enum HealthMetricType { BloodPressure = 0, BloodSugar = 1, Weight = 2, HeartRate = 3, Temperature = 4, OxygenSaturation = 5 }
public enum SubscriptionPlan { None = 0, Basic = 1, Premium = 2, Family = 3 }
