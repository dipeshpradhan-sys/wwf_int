        $(document).ready(function () {
            $("#btnResetPa").click(function (e) {
                e.preventDefault();
                var btnn = "btnResetPa";
                var messageDiv = document.getElementById("message");
                if (!messageDiv) return; // safety check
                messageDiv.style.display = "none";
                messageDiv.textContent = "";
                messageDiv.className = "error";

                var id = document.getElementById("Id").value.trim();
                var ud = document.getElementById("user_id").value.trim();
                var un = document.getElementById("txtun").value.trim();
                var np = document.getElementById("npwd").value.trim();
                var ep = document.getElementById("cpwd").value.trim();
                var cp = document.getElementById("txtcaptcha").value.trim();
                var tk = document.getElementById("tk").value.trim();

                if (!un) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_username; return false;}
                if (!np) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_new_password; focusthis("npwd"); return false;}
                if (!ep) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_confirm_password; focusthis("cpwd"); return false;}
                var pwd_fault = validatePassword(np);
                if (pwd_fault != "") { messageDiv.style.display = "block"; messageDiv.textContent = pwd_fault; focusthis("npwd"); return false;}
                if (np != ep) { messageDiv.style.display = "block"; messageDiv.textContent = msg_new_cpwd_not_same; focusthis("cpwd"); return false;}
                if (!cp) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_captcha;focusthis("txtcaptcha"); return false;}
                
                enableDisableButton(true, btnn,"")

                let data = {
                    Id : id,
                    UserId : ud,
                    Username: un,
                    Password: np,
                    ConfirmPassword: ep,
                    Token: tk,
                    Captcha: cp
                };
                //alert(JSON.stringify(data, null, 2));
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
                            messageDiv.className = "success";
                            result = JSON.parse(response);
                        }
                        // Check result
                        //alert(JSON.stringify(result));
                        if (!result) {
                            enableDisableButton(false, btnn, "");
                            messageDiv.style.display = "block";
                            messageDiv.className = "error";
                            messageDiv.textContent = err_result_null;
                            return;
                        }
                        if (result.success == true ){
                            messageDiv.className = "success";
                            //refresh page or provide message
                            messageDiv.className = "success";
                            messageDiv.textContent = result.message;
                            document.getElementById("txtcaptcha").value="";
                            refreshCaptcha();
                            messageDiv.style.display = "none";
                            document.getElementById("divResetForgotPre").style.display="none";
                            document.getElementById("divResetForgotPos").style.display="block";
                        } else{
                            enableDisableButton(false, btnn, "");
                            messageDiv.className = "error";
                            messageDiv.style.display = "block";
                            messageDiv.textContent = result.message;

                            document.getElementById("txtcaptcha").value="";
                            refreshCaptcha();
                        }
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
