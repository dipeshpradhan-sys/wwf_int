$(document).ready(function () {
    $("#btnforgotpa").click(function (e) {
        e.preventDefault();
        var btnn = "btnforgotpa";
        var messageDiv = document.getElementById("message");
        if (!messageDiv) return; // safety check
        messageDiv.style.display = "none";
        messageDiv.textContent = "";
        messageDiv.className = "error";

        var un = document.getElementById("txtun").value.trim();
        var em = document.getElementById("txtemail").value.trim();
        var cp = document.getElementById("txtcaptcha").value.trim();

        if (!un) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_username;focusthis("txtun"); return false;}
        if (!em) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_valid_email; focusthis("txtemail"); return false;}
        if (!isValidEmail(em)) { messageDiv.style.display = "block"; messageDiv.textContent =msg_enter_valid_email; focusthis("txtemail"); return false;}
        if (!cp) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_captcha;focusthis("txtcaptcha"); return false;}

        enableDisableButton(true, btnn, "");

        let data = {
            Username: un,
            Email: em,
            Captcha: cp
        };

        $.ajax({
            url: postforwardurl,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data),
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            success: function (response) {
                // If response is string convert to JSON
                let result = response;
                if (typeof response === "string") {
                    result = JSON.parse(response);
                    document.getElementById("txtun").value="";
                    document.getElementById("txtemail").value="";
                }
                // Check result
                if (!result) {
                    messageDiv.style.display = "block";
                    enableDisableButton(false, btnn, "");
                    messageDiv.className = "error";
                    messageDiv.textContent = err_result_null;
                    return;
                }
                if (result.success === true ){
                    messageDiv.className = "success";
                } else{
                    enableDisableButton(false, btnn, "");
                    messageDiv.className = "error";
                }
                messageDiv.style.display = "block";
                messageDiv.textContent = result.message;

                document.getElementById("txtcaptcha").value="";
                refreshCaptcha();

            },
            error: function (xhr, status, error) {
                enableDisableButton(false, btnn, "");
                messageDiv.className = "error";
                messageDiv.style.display = "block";
                messageDiv.textContent = xhr.responseText;

                document.getElementById("txtcaptcha").value="";
                refreshCaptcha();
            }

        });


    });

});
