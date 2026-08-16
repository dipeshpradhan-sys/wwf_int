        $(document).ready(function () {

            $("#btnlogin").click(function (e) {

                e.preventDefault();

                var messageDiv = document.getElementById("message");
                if (!messageDiv) return; // safety check
                messageDiv.style.display = "none";
                messageDiv.innerText = "";
                messageDiv.className = "error";

                var ui = document.getElementById("txtUId").value.trim();
                var un = document.getElementById("txtun").value.trim();
                var pw = document.getElementById("txtpw").value.trim();

                if (!un) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_username; return false;}
                if (!pw) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_pin; focusthis("txtpw"); return false;}

                let data = {
                    UserId : ui,
                    Username: un,
                    Pin: pw
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
                            result = JSON.parse(response);
                        }
                        // Check result
                        
                        if (!result) {
                            messageDiv.style.display = "block";
                            messageDiv.textContent = err_result_null;
                            return;
                        }
                        if (result.success === true ){
                            window.location.href = result.redirectUrl;
                        } else{
                            messageDiv.style.display = "block";
                            messageDiv.textContent = result.message;                       
                            document.getElementById("txtpw").value="";
                        }
                    },
                    error: function (xhr, status, error) {
                        messageDiv.style.display = "block";
                        messageDiv.textContent = xhr.responseText;
                        document.getElementById("txtpw").value="";
                    }
                });

            });

        });
