        $(document).ready(function () {
            $("#btnPinReset").click(function (e) {
                e.preventDefault();
                var btnn = "btnPinReset";
                var messageDiv = document.getElementById("message");
                if (!messageDiv) return; // safety check
                messageDiv.style.display = "none";
                messageDiv.textContent = "";
                messageDiv.className = "error";

                var id = document.getElementById("Id").value.trim();
                var ud = document.getElementById("user_id").value.trim();
                var un = document.getElementById("txtun").value.trim();
                var np = document.getElementById("npin").value.trim();
                var ep = document.getElementById("cpin").value.trim();
                var cp = document.getElementById("txtcaptcha").value.trim();
                var tk = document.getElementById("tk").value.trim();

                if (!un) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_username; return false;}
                if (!np) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_new_pin; focusthis("npin"); return false;}
                if (!ep) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_confirm_pin; focusthis("cpin"); return false;}
                var pin_fault = validatePinCode(np);
                if (pin_fault != "") { messageDiv.style.display = "block"; messageDiv.textContent = pin_fault; focusthis("npin"); return false;}
                if (np != ep) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_same_pin; focusthis("cpin"); return false;}
                if (!cp) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_captcha;focusthis("txtcaptcha"); return false;}

                enableDisableButton(true, btnn, "")

                let data = {
                    Id : id,
                    UserId : ud,
                    Username: un,
                    Pin: np,
                    ConfirmPin: ep,
                    Token: tk,
                    Captcha: cp
                };
                //alert(JSON.stringify(data, null, 2));
                $.ajax({
                    url: postforwardurl,
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify(data),
                    headers: {"RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                    success: function (response) {
                        // If response is string convert to JSON
                        const result = (typeof response === "string") ? JSON.parse(response) : response;

                        // Check result
                        if (!result) {
                            enableDisableButton(false, btnn, "");
                            showLError(err_result_null);
                            return;
                        }
                        if (result.success == true ){
                            messageDiv.className = "success";
                            messageDiv.textContent = result.message;
                            document.getElementById("txtcaptcha").value="";
                            refreshCaptcha();
                            messageDiv.style.display = "none";
                            document.getElementById("divResetForgotPre").style.display="none";
                            document.getElementById("divResetForgotPos").style.display="block";
                        } else{
                            enableDisableButton(false, btnn, "");
                            showLError(result.message);
                            document.getElementById("txtcaptcha").value="";
                            refreshCaptcha();
                        }
                    },
                    error: function (xhr) {
                        enableDisableButton(false, btnn, "");
                        showLError(xhr.responseText);
                        document.getElementById("txtcaptcha").value="";
                        refreshCaptcha();
                    }
                });
                // Helper
                function showLError(msg, focusId) {
                    messageDiv.className = "error";
                    messageDiv.style.display = "block";
                    messageDiv.textContent = msg;
                    if (focusId) focusthis(focusId);
                    return false;
                }
            });
        });