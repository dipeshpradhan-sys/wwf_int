
$(document).ready(function () {
		$("#btnChangePa").click(function (e) { //**button name**/
			e.preventDefault();
			var btnn = "btnChangePa";
				var messageDiv = document.getElementById("message");
				if (!messageDiv) return; // safety check
				messageDiv.style.display = "none";
				messageDiv.textContent = "";
				messageDiv.className = "error";

				//** from here **/
				var md = document.getElementById("mode").value.trim();
				var ui = document.getElementById("Id").value.trim();
				var un = document.getElementById("username").value.trim();
				var op = document.getElementById("opwd").value.trim();
				var np = document.getElementById("npwd").value.trim();
				var cp = document.getElementById("cpwd").value.trim();

				if (!ui) {messageDiv.style.display = "block"; messageDiv.textContent = msg_blank_username; return false;}
				if (!op) {messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_old_password; focusthis("opwd"); return false;}
				if (!np) {messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_new_password; focusthis("npwd"); return false;}
				if (!cp) {messageDiv.style.display = "block"; messageDiv.textContent = msg_enter_confirm_password; focusthis("cpwd"); return false;}
				var pwd_fault = validatePassword(np);
				if ( pwd_fault != "" ){messageDiv.style.display = "block"; messageDiv.textContent = pwd_fault; focusthis("npwd"); return false;}
				if (op == np) {messageDiv.style.display = "block"; messageDiv.textContent = msg_old_new_pwd_same; focusthis("npwd"); return false;}
				if (np != cp) {messageDiv.style.display = "block"; messageDiv.textContent = msg_new_cpwd_not_same; focusthis("cpwd"); return false;}

				enableDisableButton(true, btnn,"")

				let data = {
						Mode: md,
						UserId : ui,
						Username: un,
						OldPassword : op,
						NewPassword: np,
						ConfirmPassword: cp
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
										enableDisableButton(false, btnn, ""); //****/
										messageDiv.style.display = "block";
										messageDiv.className = "error";
										messageDiv.textContent = err_result_null;
										return;
								}
								if (result.success == true ){

										//** from here **/
										document.getElementById("opwd").value  = "";
										document.getElementById("npwd").value = "";
										document.getElementById("cpwd").value = "";
										document.getElementById("password_strength_text").textContent = "";
										document.getElementById("password_strength_color").style.background = "none";
										//** to here **/

										editcancel(postbacktitle);
										messageDiv.className = "success";
										messageDiv.textContent = result.message;
										messageDiv.style.display = "block";
										enableDisableButton(false, btnn, "");
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
