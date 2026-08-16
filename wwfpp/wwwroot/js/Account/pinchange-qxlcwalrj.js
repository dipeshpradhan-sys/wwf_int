

$(document).ready(function () {
	$("#btnPinChange").click(function (e) { //**button name**/
				e.preventDefault();
				var btnn = "btnPinChange";
				var messageDiv = document.getElementById("message");
				if (!messageDiv) return; // safety check
				messageDiv.style.display = "none";
				messageDiv.textContent = "";
				messageDiv.className = "error";

				//** from here **/
				var md = document.getElementById("mode").value.trim();
				var ui = document.getElementById("Id").value.trim();
				var un = document.getElementById("username").value.trim();
				var pd = document.getElementById("pwd").value.trim();
				var np = document.getElementById("npin").value.trim();
				var cp = document.getElementById("cpin").value.trim();

			if (!ui) { messageDiv.style.display = "block"; messageDiv.textContent = msg_blank_username; return false;}
			if (!pd) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_password; focusthis("pwd"); return false;}
			if (!np) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_new_pin; focusthis("npin"); return false;}
			if (!cp) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_confirm_pin; focusthis("cpin"); return false;}
				var pin_fault = validatePinCode(np);
			if (pin_fault != "") { messageDiv.style.display = "block"; messageDiv.textContent = pin_fault; focusthis("npin"); return false;}
			if (np != cp) { messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_same_pin; focusthis("cpin"); return false;}

			enableDisableButton(true, btnn,"")

				let data = {
						Mode: md,
						UserId : ui,
						Username: un,
						Password : pd,
						NewPin: np,
						ConfirmPin: cp
				};
				//** to here **/

				$.ajax({
						url: postforwardurl, //** page name and ctrl**/
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
									enableDisableButton(false, btnn, ""); //**page**/
										messageDiv.style.display = "block";
										messageDiv.className = "error";
										messageDiv.textContent = err_result_null;
										return;
								}
								if (result.success == true ){
										//** from here **/
										document.getElementById("pwd").value  = "";
										document.getElementById("npin").value = "";
										document.getElementById("cpin").value = "";
										document.getElementById("pin_strength_text").textContent = "";
										document.getElementById("pin_strength_color").style.background = "none";
										//** to here **/

										editcancel(postbacktitle);
										messageDiv.className = "success";
										messageDiv.textContent = result.message;
										messageDiv.style.display = "block";
									enableDisableButton(false, btnn, ""); //**page**/

								} else{
									enableDisableButton(false, btnn, "");
										messageDiv.className = "error";
										messageDiv.style.display = "block";
										messageDiv.textContent = result.message;
								}
						},
						error: function (xhr, status, error) {
								enableDisableButton(false, "btnlogin", "");
								messageDiv.className = "error";
								messageDiv.style.display = "block";
								messageDiv.textContent = xhr.responseText;
						}
				});/*ajax end*/

		}); // btn click
}); //document end