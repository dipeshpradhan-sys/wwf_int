/*******************************************************************************
* Package    : PandaPays
* Author     : eFour Core Pvt. Ltd.
* Author URI : https://www.efourcore.com.np/
* Page       : js/lang.js
* Description: maintain messages or labels
* Date       : 30-May-2026
* Update	 : 30-May-2026
*******************************************************************************
*******************************************************************************
* NOTE FOR TRANSLATING THIS FILE
*******************************************************************************
* DO NOT CHANGE THE TEXT OF LEFT HAND SIDE OF THE = (EQUAL) SIGN
* ONLY TRANSLATE THE TEXT OF RIGHT HAND SIDE OF = (EQUAL) SIGN within the "" (double quotation)
* DO NOT USE ' (SINGLE QUOTATION) OR " (DOUBLE QUOTATION) MARK
* DO NOT CHANGE THE TEXT WITH <[ ]>. EG : <[FISCAL-YEAR]> - <[FISCAL-YEAR]> NEED NOT TO BE CHANGED'
* DO NOT CHANGE THE TEXT WITH <>. EG : <H1> - H1 NEED NOT TO BE CHANGED'
*******************************************************************************/

/*----------------------------------------------------------------------------------------------------------*
 * MESSAGES FOR THE APPLICATION
 *----------------------------------------------------------------------------------------------------------*/
/* GLOBAL */
var please_wait = "Please wait ...";
var StrInitialMsg = "Alert !!! Check the following fields\n---------------------------------------------------";
var err_in_process = "Error happened during processing.";
var err_result_null = "Result is undefined/null";
var msg_preview = "Preview";
var msg_image_preview = "Image Preview";
var msg_close_preview = "Close Preview";
var msg_hours = "Hours";
//HOURS/MINUTE
var msg_lbl_hrs = "--Hour--";
var msg_lbl_min = "--Minute--";
var msg_enter_valid_email = "Enter valid email.";

var msg_clear = "Clear";
var msg_the_value = "The value ";
var msg_is_not_valid_date = " is not a vaild date.\n\r";
var msg_is_not_valid_date_format = " is not in a vaild date format.\n\r";
var msg_select_status = "Select status.";
var msg_select_sorting = "Select sorting.";
var msg_select_employee = "Select employee.";
var msg_select_fiscal_year = "Select fiscal year.";
var msg_select_start_fiscal_year = "Select start fiscal year.";
var msg_select_end_fiscal_year = "Select end fiscal year.";
var msg_select_year = "Select year.";
var msg_select_start_year = "Select start year.";
var msg_select_end_year = "Select end year.";
var msg_select_month = "Select month.";
var msg_select_start_month = "Select start month.";
var msg_select_end_month = "Select end month.";
var msg_select_week = "Select week.";
var msg_select_start_week = "Select start week.";
var msg_select_end_week = "Select end week.";
var msg_select_bi_week = "Select bi-week.";
var msg_select_start_bi_week = "Select start bi-week.";
var msg_select_end_bi_week = "Select end bi-week.";
var msg_select_empoloyee_payslip = "Please select at least one employee before previewing payslip.";

var msg_select_valid_csv_file = "Select valid CSV file.";
var msg_select_checkbox_to_activate = "Select checkbox to activate.";
var msg_select_checkbox_to_delete = "Select checkbox to delete.";
var msg_select_at_least_one_checkbox = "Select at least one checkbox to perform this action.";
var msg_select_at_least_one_checkbox_to_activate = "Select at least one checkbox to activate.";
var msg_select_at_least_one_checkbox_to_deactivate = "Select at least one checkbox to deactivate.";
var msg_select_at_least_one_checkbox_to_delete = "Select at least one checkbox to delete.";
var msg_select_at_least_one_employee_to_update = "Select at least one employee to update.";
var msg_select_at_least_one_record_to_update = "Select at least one record to update.";
var msg_select_showing_option = "Select showing option.";

var msg_enter_only_numeric_value = "Enter only numeric value.";
var msg_enter_only_alphabets_value = "Enter only alphabet value.";
var msg_enter_start_date = "Enter start date.";
var msg_enter_end_date = "Enter end date.";
var msg_please_enter_valid_date_format = "Enter a valid date in the format dd/mm/yyyy.";
var msg_please_enter_valid_date_format_mm = "Enter a valid date in the format mm/dd/yyyy.";
var msg_please_enter_date_in_the_format = "Enter date in the format dd/mm/yyyy.";
var msg_please_enter_date_in_the_format_mm = "Enter date in the format mm/dd/yyyy.";

var msg_select_at_least_one_checkbox_to_download = "Select at least one checkbox to download.";
var msg_select_at_least_one_checkbox_to_change_status = "Select at least one checkbox to change timesheet status.";
var msg_calculate_before_save = "Please click Calculate before Save.";
/* CONFIRMATION */
var msg_are_you_sure_to_delete = "Are you sure you want to delete ?";
var msg_are_you_sure_to_activate = "Are you sure you want to activate ?";
var msg_are_you_sure_to_deactivate = "Are you sure you want to deactivate ?";
var msg_are_you_sure_to_clear = "Are you sure you want to clear ?";
var msg_are_you_sure_to_perform_this_action = "Are you sure you want to perform this action ?\n\nClick OK button to perform this action or Cancel button to cancel process.";
var msg_are_you_sure_to_upload_selected_file = "Are you sure you want to upload selected file ?\n\nClick OK button to upload file or Cancel button to cancel process.";

/*----------------------------------------------------------------------------------------------------------*
 * USER ADMINISTRATION >>
 *----------------------------------------------------------------------------------------------------------*/
var username_policy = "Username must be at least 4 - 50 characters long, will accept A-Z, a-z, 0-9 and some punctuation(special) character( _, @, ., -). Please avoid space and/or unicode characters. ";
var password_policy = "Password must be at least 8 - 20 characters long. For Strong Password, it must have at least one upper case letter, one lower case letter,  one number and one punctuation(special) character(~ ` ! @ # $ % ^ & * ( ) - + = { } [ ] etc) . Please avoid space and/or unicode characters. ";
var pincode_policy = "Pin Code must be 6 digits random number. For strong pin code, avoid consecutive and/or repeating digits.";
var err_captcha_failed = "Fail, Captcha and entered text did not match.";
var msg_enter_username = "Enter username.";
var msg_blank_username = "No username.";
var msg_enter_password = "Enter password.";
var msg_enter_old_password = "Enter old password.";
var msg_enter_new_password = "Enter new password.";
var msg_enter_confirm_password = "Enter confirm password.";
var msg_enter_same_password = "Enter same password.";
var msg_invalid_username = "User name is not valid.";
var msg_select_user_level = "Select user level.";
var msg_old_new_pwd_same = "Old and New password are same. Please provide new one on new password.";
var msg_new_cpwd_not_same = "New password and confirm password are not same.";
var msg_new_pwd_used_in_past = "The new password provided has been used in the past. Please provide a password that is different from your last 5 passwords";
var msg_username_have_whitespace = "Username must not contain Whitespaces.";
var msg_username_have_unicode = "Username must not contain Unicode.";
var msg_username_have_invalid_chars = "Username contain disallowed Character.";
var msg_username_have_invalid_len = "Username must be 4-50 Characters Long.";
var msg_password_have_space = "Password must not contain Whitespaces.";
var msg_password_have_unicode = "Password must not contain Unicode.";
var msg_password_have_no_ucase = "Password must have at least one Uppercase Character.";
var msg_password_have_no_lcase = "Password must have at least one Lowercase Character.";
var msg_password_have_no_digit = "Password must have at least one Digit.";
var msg_password_have_no_spc = "Password must have at least one special Character.";
var msg_password_have_no_length = "Password must be 8-20 Characters Long.";
var msg_enter_captcha = "Enter CAPTCHA.";
var msg_enter_pin = "Enter Pin code.";
var msg_enter_old_pin = "Enter old Pin Code.";
var msg_enter_new_pin = "Enter new Pin Code.";
var msg_enter_confirm_pin = "Enter confirm Pin Code.";
var msg_enter_same_pin = "New Pin Code and Confirm Pin Code are not same.";
var msg_pin_code_should_have = "Pin Code should have 6 digits.";
var msg_pin_code_numeric_digits = "Pin Code should be numeric digits only.";
var msg_enter_login_step = "Login Step is required.";
var msg_one_step_msg = "You are Switching Login method to One Step Login method. It may be easy while Login but will have low Security which is not highly recommended.\n\n";
var msg_invalid_pin = "Invalid Pin Code";
var msg_old_new_pin_same = "Old and New Pin Codes are same. Please provide new one on new Pin Code.";
var msg_rslt_pwd_policy = "Weak Password";
var msg_rslt_pin_policy = "Weak Pin";
var msg_enter_same_pin = "New Pin Code and Confirm Pin Code are not same.";
var lbl_multi_login_emotp_sub = "A unique code will be sent to your registered email address for every login attempt.";
var lbl_multi_login_fixed_sub = "Define a personal PIN that you will use consistently for login.";
var msg_select_login_step = "Select Login Step.";
/**
 * Forgot password
 */
var err_inv_user_email = "Invalid Information.<br /><br /> Provide correct information and try again.";
var msg_suc_forgot_password = "Password Changed Successfully. You should soon receive an email with your login information. Please make sure to check your spam or junk folders if you can't find the email";
var err_forgot_pass_mail_fail = "Fail, there is problem sending email. Please contact with your server administrator.";
/**
 * Forgot PIN
 */
var msg_suc_forgot_pin = "Pin Code has been reset. You should soon receive an email with your login information. Please make sure to check your spam or junk folders if you can't find the email";
var err_inv_user_change_pin = "Invalid Email or Password or Old PIN Code."; 

var msg_data_exported_success = "Data exported Successfully.";
var msg_cant_edit_overtime_salary_processed = "Salary has already been processed for the selected period. You can't edit the overtime for this period.";

/*--------------------------------------------------------------------------------'
'* Payroll Administration > Dependent Allowance Distribution'
'*--------------------------------------------------------------------------------*/
var msg_dependent_allowance_cleared_scuccessfully = "Dependent allowance cleared successfully";
var msg_update_success = "Record(s) updated successfully."
var msg_some_error_occcured_please_try_again = "Some error has been occcured while processing the page! Please try again.";

/*--------------------------------------------------------------------------------'
'* Payroll Administration > LEAVE ACCRUAL'
'*--------------------------------------------------------------------------------*/
var msg_leave_accrual_cleared_scuccessfully = "Leave accrual cleared successfully";

/*--------------------------------------------------------------------------------'
'* Payroll Administration > GRATUITY ACCRUAL'
'*--------------------------------------------------------------------------------*/
var msg_gratuity_accrual_cleared_successfully = "Gratuity accrual cleared successfully";


var msg_date_required = "Date is required.";




