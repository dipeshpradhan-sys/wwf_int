/*--------------------------------------------------------------------------------'
LangEmail.cs
--------------------------------------------------------------------------------'*/



//Access on Controller as " LangEmail.
public static class LangEmail
{
    /*
     * For Email
     */
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public const string TXT_APPROVE = "Approve";
    public const string TXT_DECLINE = "Decline";
    public const string TXT_RECOMMEND = "Recommend";
    public const string TXT_CLICK_HERE = "Click here";

    public const string EMAIL_APPROVE_DECLINE = "Please click Approve or Decline link provided below as appropriate.<br/><br/><[APPROVE-LINK]><[DECLINE-LINK]>";
    public const string EMAIL_ATTN_DO_NO_REPLY = "<br/><br/><b><i><font color=\"#B70000\" size=\"4\">ATTN : Please do not <u>reply</u> to this email. This is a system generated email sent by the system email address . This email address is not monitored and you will not receive any response.</font></i></b><br/><br/>";

    /*--------------------------------------------------------------------------------'
    * Email Pin code for Second Step verification
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_SUBJECT = "<[SITE-TITLE]> - Multi-Factor Authentication code for <[ORG-NAME]>";
    public static readonly string EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_MESSAGE = $@"
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
    public static readonly string EMAIL_ACCOUNT_FORGOT_PWD_LINK_SUBJECT = "<[SITE-TITLE]> - Reset Password - <[ORG-NAME]>";
    public static readonly string EMAIL_ACCOUNT_FORGOT_PWD_LINK_MESSAGE = $@"
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
    public static readonly string EMAIL_ACCOUNT_PWD_RESETED_SUBJECT = "<[SITE-TITLE]> - Notification of Password Change - <[ORG-NAME]>";
    public static readonly string EMAIL_ACCOUNT_PWD_RESETED_MESSAGE = $@"
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
    public static readonly string EMAIL_ACCOUNT_FORGOT_PIN_LINK_SUBJECT = "<[SITE-TITLE]> - Reset Pin - <[ORG-NAME]>";
    public static readonly string EMAIL_ACCOUNT_FORGOT_PIN_LINK_MESSAGE = $@"
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
    public static readonly string EMAIL_ACCOUNT_PIN_RESETED_SUBJECT = "<[SITE-TITLE]> - Notification of Pin Change - <[ORG-NAME]>";
    public static readonly string EMAIL_ACCOUNT_PIN_RESETED_MESSAGE = $@"
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
    public static readonly string EMAIL_SALARY_PAY_SLIP_SEND_SUBJECT = "<[SITE-TITLE]> - Salary Pay Slip of period <[MONTH-NAME]>";
    public static readonly string EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    This is to inform you that your salary for period <[MONTH-NAME]> has been processed.<br/><br/>
    Please login to <[SITE-TITLE]> to view the salary pay slip in detail.<br/><br/>
    <[TYPED-MESSAGE]><br/><br/>
    <[VIEW-LINK]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    public static readonly string EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE_BLK = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    This is notification about your salary pay slip of period <[MONTH-NAME]><br/><br/>
    Your pay slip has been blocked due to some reason. Please contact Accounts Unit for more details.<br/><br/>
    <[TYPED-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------'
    * CANCELLATION REQUEST- DISCARD'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_DISCARD_SUBJECT = "<[SITE-TITLE]> - Leave cancellation request discarded by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_DISCARD_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please discard previously submitted following Leave cancellation request of <[EMPLOYEE-NAME]> below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_DETAIL_MESSAGE = $@"
    <b>Leave Type: </b><[LEAVE-TYPE-NAME]><br/>
    <b>Submit Date: </b><[SUBMIT-DATE]><br/>
    <b>Leave From Date: </b><[LEAVE-FROM-DATE]><br/>
    <b>Leave To Date: </b><[LEAVE-TO-DATE]><br/>
    <b>Leave hours: </b><[LEAVE-HOURS]><br/>
    <b>Reason for Leave: </b><br/>
    <[LEAVE-DESCRIPTION]><br/>
    <b>Remarks: </b><br/>
    <[APP-REMARKS]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_APP_MESSAGE = $@"
    <b>Leave Type: </b><[LEAVE-TYPE-NAME]><br/>
    <b>Submit Date: </b><[SUBMIT-DATE]><br/>
    <b>Leave From Date: </b><[LEAVE-FROM-DATE]><br/>
    <b>Leave To Date: </b><[LEAVE-TO-DATE]><br/>
    <b>Leave hours: </b><[LEAVE-HOURS]><br/>
    <b>Reason for Leave: </b><br/>
    <[LEAVE-DESCRIPTION]>";
    /*--------------------------------------------------------------------------------'
    * LEAVE - SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_SAVE_SUBJECT = "<[SITE-TITLE]> - Leave submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find leave request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE - UPDATE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_UPDATE_SUBJECT = "<[SITE-TITLE]> - Change in leave submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_UPDATE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find changed leave request below<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE - RE NOTIFICATION'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_RE_NOTIFY_SUBJECT = "<[SITE-TITLE]> - Re notification of leave submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_RE_NOTIFY_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find re-notification of leave request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE - APPROVED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_APPROVED_SUBJECT = "<[SITE-TITLE]> - Leave of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_APPROVED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Leave request has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_APPROVED_MESSAGE_HR = $@"
    Dear Sir/Madam,<br/><br/>
    Leave of <[EMPLOYEE-NAME]> has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE - DICLINED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_DECLINED_SUBJECT = "<[SITE-TITLE]> - Leave of <[EMPLOYEE-NAME]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_DECLINED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Leave request has been declined.<br/><br/>
    <[STR-MESSAGE]><br/><br/> 
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE CANCELLATION REQUEST- SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_SAVE_SUBJECT = $@"
    <[SITE-TITLE]> - Leave cancellation request submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find leave cancellation request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE CANCELLATION REQUEST - RE NOTIFICATION'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_RE_NOTIFY_SUBJECT = $@"
    <[SITE-TITLE]> - Re notification of leave cancellation request submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_RE_NOTIFY_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find re-notification of leave cancellation request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE CANCELLATION REQUEST - APPROVED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_APPROVED_SUBJECT = $@"
    <[SITE-TITLE]> - Leave cancellation request of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_APPROVED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following leave cancellation request has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_APPROVED_MESSAGE_HR = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Leave cancellation request of <[EMPLOYEE-NAME]> has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * LEAVE CANCELLATION REQUEST - DICLINED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_DECLINED_SUBJECT = $@"
    <[SITE-TITLE]> - Leave cancellation request of <[EMPLOYEE-NAME]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_LEAVE_CAN_DECLINED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following leave cancellation request has been declined.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    * OVERTIME'
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_DETAIL_MESSAGE = $@"
    <b>Overtime date: </b><[OVERTIME-DATE]><br/>
    <b>Submit date: </b><[SUBMIT-DATE]><br/>
    <b>Total hour(s): </b><[TOTAL-HOURS]><br/>
    <b>Reason/Description: </b><[REASON-DESCRIPTION]><br/>
    <b>Requested by: </b><[REQUESTED-BY]>";
    public static readonly string EMAIL_EMPLOYEE_OT_REQ_REMARKS = "<br/><b>Requested by Remarks: </b><[REQUESTED-BY-REMARKS]>";
    public static readonly string EMAIL_EMPLOYEE_OT_APP_REMARKS = "<br/><b>Approved by Remarks: </b><[APPROVED-BY-REMARKS]>";

    /*--------------------------------------------------------------------------------'
    * OVERTIME - SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_SAVE_SUBJECT = "<[SITE-TITLE]> - Overtime submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_OT_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find overtime request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * OVERTIME - UPDATE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_UPDATE_SUBJECT = "<[SITE-TITLE]> - Change in overtime submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_OT_UPDATE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find changed overtime request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * OVERTIME - RE NOTIFICATION'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_RE_NOTIFY_SUBJECT = "<[SITE-TITLE]> - Re notification of overtime submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_OT_RE_NOTIFY_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find re-notification of overtime request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * OVERTIME - RECOMMENDED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_RECOMMENDED_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find recommended overtime request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * OVERTIME - APPROVED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_APPROVED_SUBJECT = "<[SITE-TITLE]> - Overtime of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_OT_APPROVED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following overtime request has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * OVERTIME - DECLINED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_OT_DECLINED_SUBJECT = "<[SITE-TITLE]> - Overtime of <[EMPLOYEE-NAME]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_OT_DECLINED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following overtime request has been declined.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please make necessary correction and send it again.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    * TIMESHEET - '
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    //EMPLOYEE TIMESHEET SEND
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_SEND_SUBJECT_TO_SUPERVISOR = "<[SITE-TITLE]> - Timesheet of <[EMPLOYEE-NAME]>";

    //#### Monthly
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_SEND_MESSAGE_TO_SUPERVISOR = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is Timesheet of employee <[EMPLOYEE-NAME]> of the period <[MONTH-NAME]> <[YEAR]>.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_SUPERVISOR_WB = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_SEND_MESSAGE_TO_SUPERVISOR_WB = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    //#### Weekly/biweekly
    public static readonly string EMAIL_EMPLOYEE_WB_TIMESHEET_SEND_MESSAGE_TO_SUPERVISOR = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";

    //EMPLOYEE TIMESHEET APP
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_SUPERVISOR = "<[SITE-TITLE]> - Re notification of Timesheet of <[EMPLOYEE-NAME>]";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_SUPERVISOR = $@"
    Dear Sir,<br/><br/>
    Attached file is Timesheet of employee <[EMPLOYEE-NAME]> of the period <[MONTH-NAME]> <[YEAR]>.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";

    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_ADMIN = "<[SITE-TITLE]> - Timesheet of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_ADMIN = $@"
    Dear Administrator,<br/><br/>
    Timesheet of employee <[EMPLOYEE-NAME]> of <[MONTH-NAME]> <[YEAR]> has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_ADMIN_WB = $@"
    Dear Administrator,<br/><br/>
    Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER = "<[SITE-TITLE]> - Timesheet of <[MONTH-NAME]> <[YEAR]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_WB = "<[SITE-TITLE]> - Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Timesheet of <[MONTH-NAME]> <[YEAR]> has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_WB = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_DECLINE = "<[SITE-TITLE]> - Timesheet of <[MONTH-NAME]> <[YEAR]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_DECLINE_WB = "<[SITE-TITLE]> - Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_DECLINE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Timesheet of <[MONTH-NAME]> <[YEAR]> has been declined. <br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Please make necessary correction and send it again.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_DECLINE_WB = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been declined. <br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Please make necessary correction and send it again.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    EMPLOYEE APPROVED TIMESHEET EDITED(MESSAGE OF APPROVING WHICH WAS ALREADY/PREVIOUSLY APPROVED TIMESHEET)
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_SEND_SUBJECT_TO_SUPERVISOR_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_WB_TIMESHEET_SEND_MESSAGE_TO_SUPERVISOR_RST = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is re-submitted Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year.
    <br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_SEND_MESSAGE_TO_SUPERVISOR_RST = $@"
    Dear Sir/Madam,<br/><br/>
    Attached file is re-submitted Timesheet of employee <[EMPLOYEE-NAME]> of the period <[MONTH-NAME]> <[YEAR]>.
    <br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_ADMIN_WB_RST = $@"
    Dear Administrator,<br/><br/>
    Re-submitted Timesheet of employee <[EMPLOYEE-NAME]> of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_ADMIN_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of <[MONTH-NAME]> <[YEAR]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_RST = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Re-submitted Timesheet of <[MONTH-NAME]> <[YEAR]> has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_WB_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_WB_RST = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Re-submitted Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    <br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_SUBJECT_TO_ACCOUNT_MANAGER = "<[SITE-TITLE]> - Re-submitted Timesheet approved of <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_MESSAGE_TO_ACCOUNT_MANAGER = $@"
    Dear Administrator,<br/><br/>
    Re-submitted Timesheet of employee <[EMPLOYEE-NAME]> of  <[MONTH-NAME]> <[YEAR]> has been approved.<br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_MESSAGE_TO_ACCOUNT_MANAGER_WB = $@"
    Dear Account Manager,<br/><br/>
    <[EMPLOYEE-NAME]> has edited his approved timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year.
    Please visit your dashboard for detail information.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    EMPLOYEE APPROVED TIMESHEET EDITED(MESSAGE OF APPROVING FIRST TIMESHEET)
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_SUBJECT_TO_ACCOUNT_MANAGER_FIRST_TIME = "<[SITE-TITLE]> - Timesheet approved of <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_MESSAGE_TO_ACCOUNT_MANAGER_FIRST_TIME = $@"
    Dear Account Manager,<br/><br/>
    <[EMPLOYEE-NAME]>'s timesheet of the period <[MONTH-NAME]>, <[YEAR]> year has been approved. 
    Please visit your dashboard for detail information.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_APPROVED_TIMESHEET_EDITED_MESSAGE_TO_ACCOUNT_MANAGER_WB_FIRST_TIME = $@"
    Dear Account Manager,<br/><br/>
    <[EMPLOYEE-NAME]>'s timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been approved. 
    Please visit your dashboard for detail information.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    DECLINE - RE-SUBMITTED
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_DECLINE_WB_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_DECLINE_WB_RST = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Re-submitted Timesheet of the period <[WEEK-NAME-DATE]>, <[FISCAL-YEAR]> fiscal year has been declined. <br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Please make necessary correction and send it again.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_SUBJECT_TO_USER_DECLINE_RST = "<[SITE-TITLE]> - Re-submitted Timesheet of <[MONTH-NAME]> <[YEAR]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_APP_MESSAGE_TO_USER_DECLINE_RST = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following Re-submitted Timesheet of <[MONTH-NAME]> <[YEAR]> has been declined. <br/><br/>
    Remarks<br/><[REMARKS]><br/><br/>
    Please make necessary correction and send it again.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TIMESHEET- 	DUE NOTIFICATION TO FILL UP SOON '
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_SUBJECT = "<[SITE_TITLE]> - Timesheet submission notification for period <[PEROID]>.";
    public static readonly string EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_MESSAGE = $@"
    Respected Staff,<br/><br/>
    Please submit your timesheet for period <[PEROID]> as soon as possible.<br/><br/> 
    Please ignore this email if you have already submitted the timesheet for the same.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    * TRAVEL - '
    *--------------------------------------------------------------------------------------------------------------------------------'*/
    /*--------------------------------------------------------------------------------'
    * TRAVEL - SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_SAVE_SUBJECT = "<[SITE-TITLE]> - Travel submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find travel request below.<br/><br/>
    <[STR-PARTICULARS-DETAIL]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL - UPDATE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_UPDATE_SUBJECT = "<[SITE-TITLE]> - Change in travel submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_UPDATE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find <u>changed</u> travel request below.<br/><br/>
    <[STR-PARTICULARS-DETAIL]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL - RE NOTIFICATION'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_RE_NOTIFY_SUBJECT = "<[SITE-TITLE]> - Re notification of travel submitted by <[EMPLOYEE-NAME]> <[REC-BY]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_RE_NOTIFY_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find <u>notification</u> of travel request below.<br/><br/>
    <[STR-PARTICULARS-DETAIL]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL - RECOMMENDED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_RECOMMENDED_SUBJECT = "<[SITE-TITLE]> - Travel of <[EMPLOYEE-NAME]> has been recommended.";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_RECOMMENDED_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find <u>recommended</u> travel request below.<br/><br/>
    <[STR-PARTICULARS-DETAIL]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL - APPROVE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_APPROVED_SUBJECT = "<[SITE-TITLE]> - Travel of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_APPROVED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following travel request has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL - DECLINE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_DECLINED_SUBJECT = "<[SITE-TITLE]> - Travel of <[EMPLOYEE-NAME]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_DECLINED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following travel request has been declined. 
    Please make necessary correction and send it again.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL CANCELLATION REQUEST- SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_SUBJECT = "<[SITE-TITLE]> - Travel cancellation request submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find travel cancellation request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL CANCELLATION REQUEST- DISCARD'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_DISCARD_SUBJECT = "<[SITE-TITLE]> - Travel cancellation request discarded by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_DISCARD_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please discard previously submitted following Travel cancellation request of <[EMPLOYEE-NAME]> below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>";
    /*--------------------------------------------------------------------------------'
    * TRAVEL CANCELLATION REQUEST - RE NOTIFICATION'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_RE_NOTIFY_SUBJECT = "<[SITE-TITLE]> - Re notification of travel cancellation request submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_RE_NOTIFY_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please find notification of travel cancellation request below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL CANCELLATION REQUEST - APPROVED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_APPROVED_SUBJECT = "<[SITE-TITLE]> - Travel cancellation request of <[EMPLOYEE-NAME]> has been approved.";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_APPROVED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following travel cancellation request has been approved.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL CANCELLATION REQUEST - DICLINED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_DECLINED_SUBJECT = "<[SITE-TITLE]> - Travel cancellation request of <[EMPLOYEE-NAME]> has been declined.";
    public static readonly string EMAIL_EMPLOYEE_TRAVEL_CAN_DECLINED_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    Following travel cancellation request has been declined.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- '
    *--------------------------------------------------------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_DETAIL_MESSAGE = $@"
    <b>Travel type: </b><[TRAVEL-TYPE]><br/>
    <b>Destination/s: </b><[DESTINATIONS]><br/>
    <b>Purpose: </b><[TRIP-PURPOSE]><br/>
    <b>Travel Date: </b><[TRAVEL-DATE]><br/>
    <b>Return Date: </b><[RETURN-DATE]><br/>
    <b>Submit Date: </b><[SUBMIT-DATE]>
    ";
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_DETAIL_MESSAGE = $@"
    <b>Travel type: </b><[TRAVEL-TYPE]><br/>
    <b>Destination/s: </b><[DESTINATIONS]><br/>
    <b>Purpose: </b><[TRIP-PURPOSE]><br/>
    <b>Submit Date: </b><[SUBMIT-DATE]>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- SUBMIT AS SETTLEMENT NOT REQUIRED'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_SAVE_SUBJECT = "<[SITE-TITLE]> - Travel settlement submitted by <[EMPLOYEE-NAME]> as settlement not required.";
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    <[EMPLOYEE-NAME]> has submitted travel settlement as not required to settle. 
    Please find some information below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information of travel that not require settlement.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- SUBMIT'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_SAVE_SUBJECT = "<[SITE-TITLE]> - Travel settlement submitted by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRST_SAVE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    <[EMPLOYEE-NAME]> has submitted travel settlement. Please find Some information below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information of settlement and verification.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- UPDATE BY TRAVEL SETTLER'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_UPDATE_SUBJECT = "<[SITE-TITLE]> - Travel settlement updated by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRST_UPDATE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    <[EMPLOYEE-NAME]> has updated your travel settlement. Please find Some information below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information of updated travel settlement.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- VERIFIED BY TRAVEL SETTLER'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_VERIFIED_SUBJECT = "<[SITE-TITLE]> - Travel settlement verified by <[EMPLOYEE-NAME]>";
    public static readonly string EMAIL_EMPLOYEE_TRST_VERIFIED_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    <[EMPLOYEE-NAME]> has verified your travel settlement request. Please find Some information below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information of verified travel settlement.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- VERIFIED BY TRAVEL SETTLER [FOR SETTLEMENT NOT REQUIRED]'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_VERIFIED_SUBJECT = "<[SITE-TITLE]> - Travel settlement verified by <[EMPLOYEE-NAME]> as settlement not required.";
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_VERIFIED_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    <[EMPLOYEE-NAME]> has verified your travel settlement request which is submitted as not required to settle. 
    Please find Some information below.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information of verified travel settlement.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * TRAVEL SETTLEMENT- 	DUE NOTIFICATION TO FILL UP SOON '
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_DUE_SUBJECT = "<[SITE-TITLE]> - Travel Settlement submission notification";
    public static readonly string EMAIL_EMPLOYEE_TRST_NTR_DUE_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Please submit your following travel(s) for settlement as soon as possible.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    Please visit to <[SITE-TITLE]> for detail information. <br/><br/>
    Please ignore this email if you have already submitted the travel(s) for settlement.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * CONTRACT ALERT
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_CONTRACT_NTR_SUBJECT = "<[SITE-TITLE]> - Employee contract expiry notification";
    public static readonly string EMAIL_EMPLOYEE_CONTRACT_NTR_MESSAGE = $@"
    Dear Sir/Madam,<br/><br/>
    Following employee's contract are going to expire or has been expired.<br/><br/>
    <[STR-MESSAGE]>Please visit to the <[SITE-TITLE]> and update as required and appropriate.<br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * STAFF UPDATE
    *--------------------------------------------------------------------------------'
    */

    public static readonly string EMAIL_EMPLOYEE_STAFF_UPDATE_SUBJECT = "<[SITE-TITLE]> - Staff Update for <[IN-OUT-DATE]>";

    public static readonly string EMAIL_EMPLOYEE_STAFF_UPDATE_MESSAGE = $@"
    Dear All,<br/><br/>
    Staff Update for <[IN-OUT-DATE]>.<br/><br/>
    <[STR-MESSAGE]><br/><br/>
    <b><i>Note: For staff update anything informed after 9:00 AM  will not be entertained.</i></b><br/><br/>
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";
    /*--------------------------------------------------------------------------------'
    * BIRTHDAY WISHES TO EMPLOYEE'
    *--------------------------------------------------------------------------------'
    */
    public static readonly string EMAIL_EMPLOYEE_HAPPY_BIRTHDAY_SUBJECT = "<[SITE-TITLE]> - Happy Birthday";
    public static readonly string EMAIL_EMPLOYEE_HAPPY_BIRTHDAY_MESSAGE = $@"
    Dear <[EMPLOYEE-NAME]>,<br/><br/>
    We at the <[ORG-NAME]> family would like to wish you a happy birthday today, 
    thank you for working with us and contributing to nature conservation. 
    Wishing you all the blessings in your career with us and the years ahead.<br/><br/>
    Happy birthday once again.<br/><br/> <[ORG-NAME]> family
    Thank You<br><[SITE-ADMIN-NAME]><br><[SITE-TITLE]> - <[ORG-NAME]><br/><br/>
    ";



#pragma warning restore CA1707 // Identifiers should not contain underscores
}