        $(document).ready(function () {
            $("#btnlogin").click(function (e) {
                e.preventDefault();
                var messageDiv = document.getElementById("message");
                if (!messageDiv) return; // safety check
                messageDiv.style.display = "none";
                messageDiv.textContent = "";
                messageDiv.className = "error";

                const un = document.getElementById("txtun").value.trim();
                const pw = document.getElementById("txtpw").value.trim();
                let captcha  = "";
                let sc = "none";
                if (!un) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_username;focusthis("txtun"); return false;}
                if (!pw) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_password; focusthis("txtpw"); return false;}
                
                const rm = $("#chkRememberMe").is(":checked") ? "1" : "0";

                if ($("#divCaptcha").is(":visible")) {
                    sc = "block";
                    captcha = $("#txtcaptcha").val().trim();
                    if (!captcha) { showLError(msg_enter_captcha, "txtcaptcha"); return; }
                }

                const data = {
                    Username: un,
                    Password: pw,
                    RememberMe: rm,
                    ShowCaptcha:sc,
                    Captcha: captcha
                };

                $.ajax({
                    url: postforwardurl,
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify(data),
                    headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                    success: function (response) {
                        const result = (typeof response === "string") ? JSON.parse(response) : response;
                        handleResult(result, messageDiv);
                    },
                    error: function (xhr) {
                        let result;
                        try { result = JSON.parse(xhr.responseText); } catch { result = null; }
                        handleResult(result, messageDiv, xhr.responseText);
                    }
                });
            });

            // Helper: show error and focus field
            function showLError(msg, focusId) {
                const messageDiv = document.getElementById("message");
                messageDiv.style.display = "block";
                messageDiv.textContent = msg;
                if (focusId) focusthis(focusId);
            }
            // Helper: apply server result consistently
            function handleResult(result, messageDiv, fallbackMsg) {
                if (!result) {
                    messageDiv.style.display = "block";
                    messageDiv.textContent = fallbackMsg || err_result_null;
                    return;
                }
                if (result.success) {
                    window.location.href = result.redirectUrl;
                } else {
                    messageDiv.style.display = "block";
                    messageDiv.textContent = result.message;
                    $("#divCaptcha").css("display", result.showcaptcha === "block" ? "block" : "none");
                    $("#chkRememberMe").prop("checked", result.rememberme === "1");
                    $("#txtcaptcha").val("");
                    refreshCaptcha();
                }
            }
        });