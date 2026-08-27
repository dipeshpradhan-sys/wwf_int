//Access on Controller as " Lang.
public static class Lang
{
    public const string SITE_ADMIN_EMAIL  = ""; /* Site Administrator  email address*/
    public const string SITE_ADMIN_NAME   = "Administrator"; /* Site Administrator  name*/
    public const string SUPER_ADMIN_EMAIL = "shivascm@hotmail.com"; /* Super Administrator  email address*/
    public const string SUPER_ADMIN_NAME  = "Super Administrator"; /* Super Administrator*/

    public const string msg_not_able_connect_db = "Not able to connect to the database server.";

    //public const string lbl_fields_required = "* indicates required field";
    public const string msg_permission_denied = "Permission Denied!";
    public const string msg_please_wait = "Please wait ...";
    public const string msg_error = "Error in process.";
    public const string msg_error_invalid = "Invalid process.";
    public const string msg_undefined = "Status Undefined.";
    public const string msg_no_record_found = "No record found.";
    public const string msg_no_record_selected = "No record(s) selected.";
    public const string msg_insufficient_info = "Insufficient information.";
    public const string msg_session_expired = "Session expired. Please try again by re-login.";
    public const string msg_added_success = "Record(s) added successfully.";
    public const string msg_update_success = "Record(s) updated successfully.";
    public const string msg_activate_success = "Record(s) activated successfully.";
    public const string msg_deactivate_success = "Record(s) deactivated successfully.";
    public const string msg_delete_success = "[<DELETED-ROWS>] Record(s) deleted successfully.";
    public const string msg_deleted_some = "[<DELETED-ROWS>] record(s) deleted and [<UN-DEL-ROWS>] record(s) not deleted. The record(s) may be in use in another place. Please check and try again.";
    public const string msg_pending_deleted_some = "[<DELETED-ROWS>] pending record(s) deleted. [<UN-DEL-ROWS>] non pending record(s), cannot delete.";
    public const string msg_delete_fail = "(0) record(s) deleted. The record may be in use in another place. Please check and try again.";
    public const string msg_pending_delete_fail = "Non pending record(s), cannot delete.";
    public const string msg_record_exist_other = "Record(s) already exist in another record.";
    public const string msg_record_already_exist = "Request already exists for selected date range!";
    public const string msg_cancel_request_success = "Cancellation request has been submitted successfully.";
    public const string msg_cancel_request_fail = "Error while submitting cancellation request.";
    public const string msg_cancel_discared_success = "Cancellation request has been discarded successfully."; 
    public const string msg_cancel_discard_fail = "Error while dicarding cancellation request.";
    public const string msg_fs_used_greater_given = "Error while storing data.Used hours is greater than provided annual hours of employee.";
    public const string msg_leave_cf_success = "Leave carry forwarded successfully.";
    public const string msg_fy_mismatch = "Mismatched in fiscal year and/or date.";
    public const string msg_csv_import_success = "CSV file imported successfully.";
    public const string msg_email_sent_success = "Email Sent  successfully.";
    public const string msg_clear_success = "Record(s) cleared successfully.";
    public const string msg_leave_already_exists = "Leave already exists on the selected date range.";
    public const string msg_leave_hour_exceed = "Leave hour(s) exceeded. Make changes on the date range.";
    public const string msg_overtime_exist = "Overtime exist for the day. Select another date";
    public const string msg_overtime_not_enough_hour = "Not enough hours available for the day.";
    public const string msg_exceeded = "Maximum limit has been exceeded.";
    public const string msg_employee_overtime_bulk_error_save = "Nothing to save. All employees have zero overtime.";
    public const string msg_payslip_qued = "[<QUED-PAYSLIP-ROWS>] payslip(s) queued for sending.";
    public const string msg_no_payslip_found = "No payslips found to queue.";
    public const string msg_emp_leave_on_dayoff = "Leave cannot be applied on employee day-off.";
    public const string msg_emp_leave_on_holiday = "Leave cannot be applied on holiday dates.";


    public const string msg_invalid_user_pwd = $@"User Name or Password is invalid. Please try again.
                If you have forgotten the password, request a new password by clicking Forgot your password? link.";
    public const string msg_invalid_user_pin = $@"Pin is invalid. Please try again.
                If you have forgotten the pin, request a new pin by clicking Forgot your pin? link.";

    public const string msg_task_complete = "Task completed successfully.";
    public const string msg_some_fields_missing = "Some field(s) are missing. Complete all required inputs to continue.";
    public const string msg_error_generate_captcha = "Error Generating Captcha";
    public const string msg_incorrect_captcha  = "Captch is incorrect. Try again.";
    public const string msg_forgot_pwd_link_success = "Password reset link sent successfully.Please check your email inbox(and spam folder if needed) for the reset instructions.";
    public const string msg_forgot_pwd_link_invalid = "User Name or Email is invalid. Please try again.";
    public const string Username_policy = "Username must be at least 4 - 50 characters long, will accept A-Z, a-z, 0-9 and some punctuation(special) character( _, @, ., -). Please avoid space and/or unicode characters. ";
    public const string Password_policy = "Password must be at least 8 - 20 characters long. For Strong Password, it must have at least one upper case letter, one lower case letter,  one number and one punctuation(special) character(~ ` ! @ # $ % ^ & * ( ) - + = { } [ ] etc) . Please avoid space and/or unicode characters. ";
    public const string Pincode_policy = "Pin Code must be 6 digits random number. For strong pin code, avoid consecutive and/or repeating digits.";
    public const string lbl_empty_subject = "No Subject";
    public const string msg_new_cpwd_not_same = "New password and confirm password are not same.";
    public const string msg_pwd_length_not_valid = "Password must be between 8 and 20 characters long.";
    public const string msg_pwd_must_not_space = "Password must not contain spaces.";
    public const string msg_pwd_must_not_unicode = "Password must not contain Unicode characters.";
    public const string msg_pwd_must_upper = "Password must contain at least one uppercase letter.";
    public const string msg_pwd_must_lower = "Password must contain at least one lowercase letter.";
    public const string msg_pwd_must_digit = "Password must contain at least one digit.";
    public const string msg_pwd_must_special = "Password must contain at least one special character.";
    public const string msg_pwd_reset_expired = "Reset Password Link invalid or already expired.";
    public const string msg_pwd_reused_detected = "The new password you entered matches one of your previous passwords. For security, please choose a password different from your last five.";
    public const string msg_password_changed_successfully = "Password changed successfully!";
    public const string msg_suc_forgot_password = "Password changed successfully. Please proceed to the login page by clicking the Login button below.";
    public const string msg_pin_reset_expired = "Reset Pin Link invalid or already expired.";
    public const string msg_suc_forgot_pin = "Pin changed successfully. Please proceed to the login page by clicking the Login button below.";
    public const string msg_new_cpin_not_same = "New pin and confirm pin are not same.";
    public const string msg_rslt_pwd_policy = "Weak Password";
    public const string msg_rslt_pin_policy = "Weak Pin";
    public const string msg_old_new_pwd_same = "Old and new password are same. Please provide new one on new password.";
    public const string msg_invalid_password = "Password is invalid. Please try again with valid password.";
    public const string msg_pin_changed_successfully = "Pin changed successfully!";
    /*--------------------------------------------------------------------------------'
    * Change Password
    *--------------------------------------------------------------------------------'
    */

    public const string msg_password_expired = "The system found your password is Expired! So you are redirected here to change password.";
    public const string msg_password_weak = "The system found you are using WEAK password. So you are redirected here to change password.";
    public const string msg_next_password_change_due = "Your most recent password change was done on <[last-updated-date]> and upcoming password change is due in <[due-date]> day(s).";
    public const string msg_password_change_overdue = "Your most recent password change was done on <[last-updated-date]> and password change schedule exceeded by <[due-date]> day(s) from specified time limit of <[due-date-limit]> days, and it is now necessary to change your password.";
    public const string msg_no_password_change_info = "You have not any password changed record.";

    public const string lbl_single_login_step = "Single Factor Login";
    public const string lbl_multi_login_step = "Two Factor Login (Recomended)";
    public const string lbl_multi_login_emotp = "Receive OTP on email";
    public const string lbl_multi_login_emotp_sub = "A unique One Time PIN (OTP) will be sent to your registered email address for every login attempt.";
    public const string lbl_multi_login_fixed = "Set fixed PIN code";
    public const string lbl_multi_login_fixed_sub = "User Defined PIN (UDP) that you will use consistently for login.";
    public const string msg_MFA_changed_successfully = "Login step updated successfully.";
    /*--------------------------------------------------------------------------------'
    * Email Pin code for Second Step verification
    *--------------------------------------------------------------------------------'*/
    public const string lbl_no_holidays_defined = "Holidays not defined for this fiscal year";
    public const string msg_err_holiday_process = "Error in process. Selected date is past date or data already exist in timesheet.";
    public const string msg_provide_date_not_within_range = "Please provide the date within fiscal year <[FISCAL-START-DATE]> and <[FISCAL-END-DATE]>.";
    public static string msg_can_not_set_holiday_weekend = "Input date is weekend.";

    /*--------------------------------------------------------------------------------'
    * EMPLOYEE FUND SOURCE
    *--------------------------------------------------------------------------------'
    */
    public static string NO_FILE_UPLOADED = "No file uploaded.";
    public static string FISCAL_DATES_NOT_FOUND_IN_SESSION = "Invalid Start date and/or End date.";
    public static string FUND_SOURCE_NOT_EXIST = "Fund source does not exist: <[FUND-SOURCE-NAME]>.";
    public static string INACTIVE_EMPLOYEE = "The employee having code <[EMP-CODE]> is inactive or doesn't exist in our database.";
    public static string USED_HRS_GREATER_THAN_PROVIDED = "Used hours is greater than provided annual hours of employee <[EMP-CODE]>.";
    public static string EMPLOYEE_FUND_SOURCE_IMPORT_SUCCESSFUL = "Employee fund source import completed successfully.";
    public static string INVALID_ANNUAL_HOURS = "Fund source annual hours should be number.";

    /*
     * For Email
     */
    public const string EMAIL_ATTN_DO_NO_REPLY = "<br/><br/><b><i><font color=\"#B70000\" size=\"4\">ATTN : Please do not <u>reply</u> to this email. This is a system generated email sent by the system email address . This email address is not monitored and you will not receive any response.</font></i></b><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * Email Pin code for Second Step verification
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_SUBJECT = "<[SITE-TITLE]> - Multi-Factor Authentication code for <[ORG-NAME]>";
    public static string EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br>
    For your security, we have generated a multi-factor authentication code to verify your login.
    Please use the code below to complete your Login.<br><br>
    Your verification code: <[PIN-CODE]><br><br>
    Do not share this code with anyone.If you did not attempt to log in, please contact the IT helpdesk immediately.
    <br><br>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * Email Forgot Password link send
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_ACCOUNT_FORGOT_PWD_LINK_SUBJECT = "<[SITE-TITLE]> - Reset Password - <[ORG-NAME]>";
    public static string EMAIL_ACCOUNT_FORGOT_PWD_LINK_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br><br>
    You have requested to change your password.
    <br><br>
    To proceed, please click the secure link below:<br>
    <[CALL-BACK-URL]>
    <br><br>
    For your security, this link will expire in 30 minutes. If you did not request to change your password, you can safely ignore this email.
    <br><br>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * Email Notification about Password reset upon successful
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_ACCOUNT_PWD_RESETED_SUBJECT = "<[SITE-TITLE]> - Notification of Password Change - <[ORG-NAME]>";
    public static string EMAIL_ACCOUNT_PWD_RESETED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br><br>
    This is to notify you that your account password has been successfully changed. If you made this change, no further action is required.<br>
    If you did not request this change, please reset your password immediately to secure your account.<br><br>
    For your security:<br>
    Do not reuse old passwords.<br>
    Choose a strong password with a mix of uppercase, lowercase, numbers, and special characters.<br><br>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";

    /*--------------------------------------------------------------------------------'
    * Email Forgot Pin link send
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_ACCOUNT_FORGOT_PIN_LINK_SUBJECT = "<[SITE-TITLE]> - Reset Pin - <[ORG-NAME]>";
    public static string EMAIL_ACCOUNT_FORGOT_PIN_LINK_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br><br>
    You have requested to change your Pin.
    <br><br>
    To proceed, please click the secure link below:<br>
    <[CALL-BACK-URL]>
    <br><br>
    For your security, this link will expire in 30 minutes. If you did not request to change your password, you can safely ignore this email.
    <br><br>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * Email Notification about Pin reset upon successful
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_ACCOUNT_PIN_RESETED_SUBJECT = "<[SITE-TITLE]> - Notification of Pin Change - <[ORG-NAME]>";
    public static string EMAIL_ACCOUNT_PIN_RESETED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br><br>
    This is to notify you that your account pin has been successfully changed. If you made this change, no further action is required.<br>
    If you did not request this change, please reset your password and pin immediately to secure your account.<br><br>
    For your security:<br>
    Do not reuse old password/pin.<br>
    Choose a strong password with a mix of uppercase, lowercase, numbers, and special characters.<br>
    Choose a strong pin with a mix of digits. Avoid repeating the same digit or simple sequences.<br><br>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * Email Notification of Pay Slip
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_SALARY_PAY_SLIP_SEND_SUBJECT = "<[SITE-TITLE]> - Salary Pay Slip of period <[MONTH-NAME]>";
    public static string EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    This is to inform you that your salary for period <[MONTH-NAME]> has been processed.<br/><br/>
    Please login to <[SITE-TITLE]> to view the salary pay slip in detail.<br/><br/>
    <[TYPED-MESSAGE]><br/><br/>
    <[VIEW-LINK]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    public static string EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE_BLK = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    This is notification about your salary pay slip of period <[MONTH-NAME]><br/><br/>
    Your pay slip has been blocked due to some reason. Please contact Accounts Unit for more details.<br/><br/>
    <[TYPED-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * TIMESHEET- 	DUE NOTIFICATION TO FILL UP SOON '
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_SUBJECT = "SITE_TITLE - Timesheet submission notification for period <[PEROID]>.";
    public static string EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_MESSAGE = $@"
    Respected Staff,<br/><br/>
    Please submit your timesheet for period <[PEROID]> as soon as possible.<br/><br/> 
    Please ignore this email if you have already submitted the timesheet for the same.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";

    public const string msg_emp_manager_not_defined = "Your Manager OR/AND Line Manager has not been defined.";
    public const string msg_leave_apply_elligible = "Sorry, you are not elligible to apply for leave.";
    public const string msg_pending_leave_exist = "Some of your applied leaves are in pending. You can apply further leaves only after approving/declining pending leaves from your approval authority.";

    /*--------------------------------------------------------------------------------'
    * Email Notification about LEAVE DISCARD
    *--------------------------------------------------------------------------------'
    */
    public static string EMAIL_EMPLOYEE_CAN_DISCARD_SUBJECT = "Leave cancellation request discarded by <[EMPLOYEE-NAME-ONLY]>";
    public static string EMAIL_EMPLOYEE_CAN_DISCARD_MESSAGE = "Dear Sir/Madam,<br/><br/>Please discard my previously submitted following Leave cancellation request below.<br/><br/><[STR-MESSAGE]><br/><br/>Regards<br/><[EMPLOYEE-NAME-ONLY]><br/><br/>";

    /*--------------------------------------------------------------------------------'
    * Email Notification about TRAVEL CANCEL & DISCARD
    *--------------------------------------------------------------------------------'
    */
    public static string CANCEL_REQUEST_SAVED_AND_EMAIL_SENT = "Cancel request saved and email sent.";
    public static string EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_SUBJECT = "Travel cancellation request submitted by <[EMPLOYEE-NAME-ONLY]>";
    public static string EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_MESSAGE = "Dear Sir/Madam,<br/><br/>Please find my travel cancellation request below.<br/><br/><[STR-MESSAGE]><br/><br/>Regards<br/><[EMPLOYEE-NAME-ONLY]><br/><br/>";
    public static string NOT_FOUND = "Record not found.";
}