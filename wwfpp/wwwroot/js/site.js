// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

let path = "/"; //if not shown define ~

// Write your JavaScript code.
const handlers = {
	loadbhayepachhi: (parm_page) => {
		document.frm.method = "post";
		document.frm.action = parm_page;
		document.frm.submit();
	},
	sayHelloToShiva: () => {
		//place holder for new fuction
		console.log("Hello Shiva!");
	}
};
/*----------------------------------------------------------------------------------------
 * //Popup
 *---------------------------------------------------------------------------------------*/
function PopUpW(parm_page) { window.open(parm_page, "prvwin", "width=650,height=400,scrollbars=yes,resizable=yes,status=0,top=0,left=0,menubar=yes"); }
function PopUp(parm_page) { window.open(parm_page, "prvwo", "width=650,height=400,scrollbars=yes,resizable=yes,status=0,top=0,left=0"); }
/*----------------------------------------------------------------------------------------*/
function modalCloseCancel(btnValue) { if (btnValue.text() === "Close") {$("#modal-message").hide(); var modal = bootstrap.Modal.getInstance(document.getElementById("ModalID")); modal.hide(); } else if (btnValue.text() === "Cancel") {$("#ModalID .modal-title").text("View"); $("#btnSave").text("Edit"); $(btnValue).text("Close"); } }
/*----------------------------------------------------------------------------------------
 * What will do by this windowresize function???
 *---------------------------------------------------------------------------------------*/
function windowresize() { $(window).resize(function () { let height = $(this).height(); if (height < 200) { table.page.len(5).draw(); } else if (height < 400) { table.page.len(10).draw(); } else if (height < 600) { table.page.len(15).draw(); } else if (height < 800) { table.page.len(30).draw(); } else { table.page.len(100).draw(); } }); }
/*----------------------------------------------------------------------------------------*/
setTimeout(function () { var modal = bootstrap.Modal.getInstance(document.getElementById("ModalID")); if (modal) modal.hide(); }, 1000);
/*----------------------------------------------------------------------------------------
 * //Validate and return control existance
 *---------------------------------------------------------------------------------------*/
function yoCtrlHo(msg_ctrl) { if (msg_ctrl == null || msg_ctrl == 'undefined' || msg_ctrl == '') { return document.getElementById('message'); } else { return document.getElementById(msg_ctrl); } }
/*----------------------------------------------------------------------------------------
 * //for showing message
 *---------------------------------------------------------------------------------------*/
function hideMessage(DivId = "") { const mDiv = yoCtrlHo(DivId); if (!mDiv) return false; /*safety check*/if (mDiv.style.display != "" || mDiv.style.display == "block") { mDiv.style.display = "none"; mDiv.textContent = ""; } return false; }
function showSuccess(msg = "", DivId = "") { const mDiv = yoCtrlHo(DivId); if (!mDiv) return false; /* safety check*/	if (msg == "") { msg = "No Message"; } mDiv.style.display = "block"; mDiv.className = "success"; mDiv.textContent = msg; return false; }
function showError(msg = "", DivId = "") { const mDiv = yoCtrlHo(DivId); if (!mDiv) return false; /*safety check*/	if (msg == "") { msg = err_in_process; } mDiv.style.display = "block"; mDiv.className = "error"; mDiv.textContent = msg; return false; }
/*----------------------------------------------------------------------------------------
 * control
 *---------------------------------------------------------------------------------------*/
function checkEnter(e, funName, parm_page) { if (e === 13 && handlers[funName]) { handlers[funName](parm_page); /* safely call the function*/ } }
function focusthis(focusthis) { document.getElementById(focusthis).focus(); }
function goNext(e, ctl) { if (e == 13) { document.getElementById(ctl).focus(); } }
function moveNextOnEnter(e) { if (e.key === "Enter") { e.preventDefault(); /* prevent form submit*/let form = e.target.form; let index = Array.prototype.indexOf.call(form, e.target);/* loop forward until we find a focusable element*/for (let i = index + 1; i < form.elements.length; i++) { let next = form.elements[i]; if (next.type !== "hidden" && !next.disabled && next.offsetParent !== null) { next.focus(); break; } } } }
/*----------------------------------------------------------------------------------------
 * show with some value help
 *---------------------------------------------------------------------------------------*/
function showHelp(path) { var divCtrl = document.getElementById('divHelp'); if (divCtrl.style.display == "" || divCtrl.style.display == "none") { document.getElementById('divHelp').style.display = "block"; document.getElementById('imgHelp').src = path + 'images/wr.png'; } else if (divCtrl.style.display != "" || divCtrl.style.display == "block") { divCtrl.style.display = "none"; document.getElementById('imgHelp').src = path + 'images/help_icon.png'; } }
/*----------------------------------------------------------------------------------------
 * 
 *---------------------------------------------------------------------------------------*/
function GetAllData(ilaka, chkValue, dynamicColumns, filters = "") {
	if (typeof hideviewaction === "undefined" || hideviewaction === null || hideviewaction === "") {
		var sTtl = "Edit"; var sImg = "edit";
		var img_icon = `<img src="${path}images/${sImg}.png" title="${sTtl}" height="16" width="16" border="0">`;
	}
	var columns = [
		{
			data: null,
			orderable: false,
			searchable: false,
			render: function (data, type, row) {
				var chkAssignValue = row[chkValue];
				var checked = (row.block_status === "Yes") ? "checked" : "";
				return '<input type="checkbox" class="row-checkbox" value="' + chkAssignValue + '" ' + checked +'>';
			},
			autoWidth: true
		},
		// ? Insert dynamic columns here
		...dynamicColumns
	];

	//Conditionally add the button column
	if (typeof hideviewaction === "undefined" || hideviewaction === null || hideviewaction === "") {
		columns.push({
			orderable: false,
			render: function (data, type, row) {
				var rowAssignValue = row[chkValue];
				return '<button type="button" name="btnView" id="btnView" class="button" data-id="'
					+ rowAssignValue + '">' + img_icon + '</button>';
			}
		});
	}
	if (typeof isFixedHeader === "undefined" || isFixedHeader === null || isFixedHeader === "") {isFixedHeader = false;}
	table = $("#tblData").DataTable({
		fixedHeader: isFixedHeader,
		processing: true,
		serverSide: true,
		searching: true,
		lengthMenu: [
			[5, 10, 15, 30, 50, 75, 100, -1],
			[5, 10, 15, 30, 50, 75, 100, 'All'],
		],
		pageLength: 10,
		language: {
			lengthMenu: "Display _MENU_ records/page",
			info: "Total <font class=\"red bold\">_TOTAL_</font> record(s) | Page <font class=\"green bold\">_PAGE_</font> of <font class=\"green bold\">_PAGES_</font> page(s)",
			zeroRecords: "No record(s) found."
		},
		ajax: {
			type: "POST",
			url: ilaka + "List",
			headers: {
				RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
			},
			dataType: "json",
			data: function (d) {
				$.each(filters, function (modelField, controlId) {
					d[modelField] = $('#' + controlId).val();
				});
			}
		},
		columns: columns,
		columnDefs: [
			{ targets: 0, orderable: false, searchable: false }, // first column
			{ targets: 1, className: 'truncate-text' }   // truncate column
		],
		createdRow: function (row, data, dataIndex) {
			// Change color depending on status
			if (data.status === "Declined" || data.status === "Passive" || data.status === "Absent") {
				$(row).addClass("light-red");
			} else if (data.status === "Active" || data.status === "Approved" || data.status === "Present") {
				$(row).addClass("light-green");
			} else if (data.status === "Pending") {
				$(row).addClass("light-yellow");
			}
			// Increase height of group rows
			if (data._groupIndex !== undefined && data._rowInGroup === 1) {
				$(row).css("height", "40px"); // adjust as needed
			}
		},
		createdRow: function (row, data, dataIndex) {
			// Change color depending on status
			if (data.critical === "Expired") {
				$(row).css("background-color", "#ff7678"); // pink
			} else if (data.critical === "High") {
				$(row).css("background-color", "#ffc2a6"); // peach
			} else if (data.status === "Medium") {
				$(row).css("background-color", "#ffe8dd"); // light peach
			} else if (data.status === "Low") {
				$(row).css("background-color", "#eeeeee"); // gray
			}
			else {
				//$(row).css("background-color", "#ffffff"); 
			}
		}
	});

	checkUncheckControl();
	setupRowCheckboxPersistence(table, selectedIds);

	$.each(filters, function (modelField, controlId) {
		$('#' + controlId).on('change keyup', function () {
			table.draw();
		});
	});
}
/*----------------------------------------------------------------------------------------*/
function GetAllDataSN(ilaka, chkValue, dynamicColumns, filters = "", groupBy = null) {
	var sTtl = "Edit"; var sImg = "edit";
	var img_icon = `<img src="${path}images/${sImg}.png" title="${sTtl}" height="16" width="16" border="0">`;

	var columns = [
		{
			data: null,
			orderable: false,
			searchable: false,
			autoWidth: true
		},
		...dynamicColumns,
	];
	//Conditionally add the button column
	if (typeof hideviewaction === "undefined" || hideviewaction === null || hideviewaction === "") {
		columns.push({
			orderable: false,
			render: function (data, type, row) {
				var rowAssignValue = row["id"];
				return '<button type="button" name="btnView" id="btnView" class="button" data-id="'
					+ rowAssignValue + '">' + img_icon + '</button>';
			}
		});
	}
	var groupCounter = 0;
	if (typeof isFixedHeader === "undefined" || isFixedHeader === null || isFixedHeader === "") {isFixedHeader = false;}
	var table = $("#tblData").DataTable({
		fixedHeader: isFixedHeader,
		processing: true,
		serverSide: true,
		searching: true,
		ordering: (groupBy === null) ? true : false, /* ordering: groupBy === null also work for disable ordering if grouping is enabled*/
		lengthMenu: [
			[5, 10, 15, 30, 50, 75, 100, -1],
			[5, 10, 15, 30, 50, 75, 100, 'All'],
		],
		pageLength: 10,
		language: {
			lengthMenu: "Display _MENU_ records/page",
			info: "Total <font class=\"red bold\">_TOTAL_</font> record(s) | Page <font class=\"green bold\">_PAGE_</font> of <font class=\"green bold\">_PAGES_</font> page(s)",
			zeroRecords: "No record(s) found."
		},
		ajax: {
			type: "POST",
			url: ilaka + "List",
			headers: {
				RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
			},
			dataType: "json",
			data: function (d) {
				$.each(filters, function (modelField, controlId) {
					d[modelField] = $('#' + controlId).val();
				});
			}
		},
		columns: columns,
		columnDefs: [
			{
				targets: 0,
				searchable: false,
				render: function (data, type, row, meta) {
					if (groupBy !== null && row._groupIndex !== undefined && row._rowInGroup !== undefined) {
						return row._groupIndex + '.' + row._rowInGroup;
					}
					return meta.row + 1; // fallback: simple SN
				}
			}
		],
		rowGroup: groupBy !== null ? {
			dataSrc: groupBy, // group by column name
			startRender: function (rows, group) {
				groupCounter++;
				var subCounter = 0;
				rows.every(function (rowIdx) {
					subCounter++;
					var data = table.row(rowIdx).data();
					data._groupIndex = groupCounter;
					data._rowInGroup = subCounter;
					table.row(rowIdx).data(data);
				});
				return groupCounter + '. ' + group;
			}
		} : false,
		createdRow: function (row, data, dataIndex) {
			// Change color depending on status
			if (data.status === "Declined" || data.status === "Passive" || data.status === "Absent" || data.status === "I")  {
				$(row).addClass("light-red");
			} else if (data.status === "Active" == "A" || data.status === "Active" || data.status === "Approved" || data.status === "Present") {
				$(row).addClass("light-green");
			} else if (data.status === "Pending" || data.status === "P") {
				$(row).addClass("light-yellow");
			}
			// Increase height of group rows
			if (data._groupIndex !== undefined && data._rowInGroup === 1) {
				$(row).css("height", "40px"); // adjust as needed
			}
		}
	});
	// Reset groupCounter before each draw, it is also reseting on page switch
	table.on('preDraw', function () { groupCounter = 0; });

	// Resolve the column index by name and Only proceed if a valid groupBy column was found
	var groupByIndex = groupBy ? table.column(groupBy + ':name').index() : null;
	if (groupByIndex !== undefined && groupByIndex !== null) {
		table.column(groupByIndex).visible(false); // hide the grouped column
	}

	$.each(filters, function (modelField, controlId) {
		$('#' + controlId).on('change keyup', function () {
			var groupCounter = 0;
			table.draw();
		});
	});
}
/*----------------------------------------------------------------------------------------
 * 
 *---------------------------------------------------------------------------------------*/
function add(e)
{
	e.preventDefault();
	e.stopPropagation();

	var payload = {
		id: 0,
		mode: "add"
	};
	if (typeof collectExtraFields === "function") {
		var extraFields = collectExtraFields();
		if (extraFields) {
			Object.assign(payload, extraFields);
		}
	}
	//console.log("Payload:", payload);
	$("#ModalBody").html("");
	document.getElementById("mode").value = "add";
	$.get(ilaka + "AddEdit", payload, function (data)
	{
		$("#ModalBody").html(data);
		$("#ModalID .modal-title").text("Add New");
		var modal = new bootstrap.Modal(document.getElementById("ModalID"));
		modal.show();
	}).fail(function (xhr) { showError("Error occured while loading record(s): " + xhr.responseText); });
}
/*---------------------------------------------------------------------------------------*/
function ViewData(id)
{
	var payload = {
		id: id,
		mode: "edit"
	};
	if (typeof collectExtraFields === "function") {
		var extraFields = collectExtraFields();
		if (extraFields) {
			Object.assign(payload, extraFields);
		}
	}
	$("#ModalBody").html("");
	document.getElementById("mode").value = "edit";
	$.get(ilaka + "AddEdit", payload, function (data)
	{
			$("#ModalBody").html(data);
			$("#ModalID .modal-title").text("View");
			var modal = new bootstrap.Modal(document.getElementById("ModalID"));
			modal.show();
	}).fail(function (xhr) {
			showError("Error occured while loading record(s): " + xhr.responseText);
	});
}
/*---------------------------------------------------------------------------------------*/
function ShowData(ilaka, payload, Title) {
	$("#ModalBody").html("");
	$.get(ilaka, { payload }, function (data) {
		$("#ModalBody").html(data);
		$("#ModalID .modal-title").text(Title);
		var modal = new bootstrap.Modal(document.getElementById("ModalID"));
		modal.show();
	}).fail(function (xhr) {
		showError("Error occured while loading record(s): " + xhr.responseText);
	});
}
/*----------------------------------------------------------------------------------------*/
function delData(parm) {
	document.getElementById("mode").value = "delete";
	if (doAnyCheckboxSelected(msg_select_at_least_one_checkbox) == false) {
		return false;
	}
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		/* Ensure selectedIds is an array of strings// Example: ["1","2","3"]//alert(Array.from(selectedIds));*/
		$.ajax({
			type: "POST", url: parm + "Delete", data: JSON.stringify({ selectedIds: [...selectedIds] }),
			/* ? must be a raw JSON array like ["1","2","3"]*/
			contentType: "application/json; charset=utf-8",
			/* ? tell server it's JSON*/dataType: "json",
			/* expect JSON back*/
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {
					/* match your controller return { status = true/false }*/
					showSuccess(data.message); $('#tblData').DataTable().ajax.reload(null, false);
				} else { showError("Error: " + data.message); $('#tblData').DataTable().ajax.reload(null, false); }
			}, error: function (xhr, status, error) { showError("Request failed: " + error); $('#tblData').DataTable().ajax.reload(null, false); }
		});
	}
}
/*---------------------------------------------------------------------------------------*/
function submitExport(ilaka, msgbox) { if (confirm(msg_are_you_sure_to_perform_this_action) == false) { return false; } var formData = $("#frm").serialize(); $.post(ilaka + "Export", formData).done(function (data) { if (data.status === "success") { showSuccess(msg_data_exported_success, msgbox); } else { showError(data.message, msgbox); } }).fail(function (xhr) { showError(xhr.responseText, msgbox); }); }
/*----------------------------------------------------------------------------------------*/
function submitSendEmail(ilaka) { if (confirm(msg_are_you_sure_to_perform_this_action)) { var formData = $("#frm").serialize(); $("#btnSend").prop("disabled", true); $.post(ilaka + "SendEmail", formData).done(function (data) { if (data.status === "success") { showSuccess(data.message); $("#btnSend").prop("disabled", false); } else { showError(data.message); $("#btnSend").prop("disabled", false); } }).fail(function (xhr) { showError(xhr.responseText, "modal-message"); $("#btnSend").prop("disabled", false); }); } }
/*---------------------------------------------------------------------------------------*/
function updateStatus(parm, status) {
	if (doAnyCheckboxSelected(msg_select_at_least_one_checkbox) == false) { return false; }
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		// Ensure selectedIds is an array of strings
		// Example: ["1","2","3"]
		//alert(Array.from(selectedIds));
		$.ajax({
			type: "POST",
			url: parm,
			data: JSON.stringify(
				{ selectedIds: [...selectedIds], "mode": "updateStatus", "hStatus": status },
			),   // ? must be a raw JSON array like ["1","2","3"]
			contentType: "application/json; charset=utf-8", // ? tell server it's JSON
			dataType: "json",                     // expect JSON back
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {   // match your controller return { status = true/false }
					showSuccess(data.message);
					$('#tblData').DataTable().ajax.reload(null, false);
				} else {
					showError("Error: " + data.message);
				}
			},
			error: function (xhr, status, error) {
				showError("Request failed: " + error);
			}
		});

	}

}
/*----------------------------------------------------------------------------------------*/
function updateDataNoChk(parm, callback=null) {
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		var payload = { mode: "updateDataNoChk" };
		if (typeof collectExtraFields === "function") {
			var extraFields = collectExtraFields();
			if (extraFields && extraFields.length > 0) {
				payload.Fields = extraFields;
			}
		}

		$.ajax({
			type: "POST",
			url: parm,
			data: JSON.stringify(payload),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {
					showSuccess(data.message);
					$('#tblData').DataTable().ajax.reload(null, false);

					// ✅ Only run callback when status is true
					if (callback === "Y") {
						IsAllDashainZero();
					}
				} else {
					showError("Error: " + data.message);
				}
			},
			error: function (xhr, status, error) {
				showError("Request failed: " + error);
			}
		});
	}
}

/*----------------------------------------------------------------------------------------*/
function updateDataJSON(parm) {
	if (doAnyCheckboxSelected(msg_select_at_least_one_checkbox) == false) { return false; }
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		var payload = {
			mode: "updateDataNoChk"
		};
		var ids = Array.from(selectedIds); // e.g. ["12","45","78"]
		if (ids.length > 0) {
			payload.selectedIds = [...selectedIds];
		}
		if (typeof collectExtraFields === "function") {
			var extraFields = collectExtraFields();
			if (extraFields && extraFields.length > 0) {
				payload.Fields = extraFields;
			}
		}

		$.ajax({
			type: "POST",
			url: parm,
			data: JSON.stringify(payload),
			contentType: "application/json; charset=utf-8", // ? tell server it's JSON
			dataType: "json",                     // expect JSON back
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {   // match your controller return { status = true/false }
					if (data.status === "success") { showSuccess(data.message); } else { showError("Error: " + data.message); }
					$('#tblData').DataTable().ajax.reload(null, false);
				} else {
					showError("Error: " + data.message);
				}
			},
			error: function (xhr, status, error) {
				showError("Request failed: " + error);
			}
		});

	}
}
/*----------------------------------------------------------------------------------------*/
function updateDataForm(parm) {
	if (doAnyCheckboxSelected(msg_select_at_least_one_checkbox) == false) { return false; }
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		var formData = new FormData($("#frm")[0]); // or whichever form you want

		var ids = Array.from(selectedIds); // e.g. ["12","45","78"]
		if (ids.length > 0) {
			ids.forEach(id => formData.append("selectedIds", id));
		}

		// Push extraFields as JSON string
		/*
		if (typeof collectExtraFields === "function") {
			var extraFields = collectExtraFields();
			if (extraFields && extraFields.length > 0) {
				formData.append("extraFields", JSON.stringify(extraFields));
			}
		}
		*/
		//for (var pair of formData.entries()) {console.log(pair[0] + ': ' + pair[1]);}//sometime we need to debug

		$.ajax({
			type: "POST",
			url: parm,
			data: formData,
			processData: false,   // important: prevent jQuery from processing data
			contentType: false,   // important: let browser set multipart/form-data
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {
					if (data.status === "success") { showSuccess(data.message); } else { showError("Error: " + data.message); }
					$('#tblData').DataTable().ajax.reload(null, false);
				} else {
					showError("Error: " + data.message);
				}
			},
			error: function (xhr, status, error) {
				showError("Request failed: " + status + '-' + error + '-' + xhr.responseText);
			}
		});
	}
}
/*----------------------------------------------------------------------------------------*/
function submitSaveUpdate(ilaka) {
	if (confirm(msg_are_you_sure_to_perform_this_action) == false) { return false; }
	var formData = $("#frm").serialize();
	$("#btnSave").prop("disabled", true);
	$("#btnC").prop("disabled", true);
	$.post(ilaka + "Save", formData).done(function (data) {
		if (data.status === "success") {
			var table = $("#tblData").DataTable();
			table.ajax.reload(function () {
				setTimeout(function () { // ? wait for DOM to settle
					if (data.id) {
						// Find the row index by matching the data.id
						var rowIdx = table.rows().eq(0).filter(function (idx) { return String(table.row(idx).data().id) === String(data.id); });

						if (rowIdx.length) {
							var rowNode = table.row(rowIdx[0]).nodes().to$();
							rowNode.addClass("highlight-row");
							setTimeout(function () { rowNode.removeClass("highlight-row"); }, 2000);
						} else {
							//console.warn("Row not found for id:", data.id);
						}
					}
				}, 100);
			}, false); // ?? keep current page

			if ($("#btnSave").text() == "Save") {
				showSuccess(data.message);
				var modal = bootstrap.Modal.getInstance(document.getElementById("ModalID"));
				if (modal) modal.hide();
				$("#btnSave").prop("disabled", false);
				$("#btnC").prop("disabled", false);
			}
			else if ($("#btnSave").text() == "Update") {
				showSuccess(data.message, "modal-message");
				$("#btnSave").text("Edit");
				$("#btnC").text("Close");
				$("#btnSave").prop("disabled", false);
				$("#btnC").prop("disabled", false);
			}
		}
		else if (data.status === "false" || data.status === "invalid" || data.status === "error") {
			showError(data.message, "modal-message");
			$("#btnSave").prop("disabled", false);
			$("#btnC").prop("disabled", false);
		}
		else {
			showError(data.message, "modal-message");
			$("#btnSave").prop("disabled", false);
			$("#btnC").prop("disabled", false);
		}
	})
		.fail(function (xhr) {
			showError(xhr.responseText, "modal-message");
			$("#btnSave").prop("disabled", false);
			$("#btnC").prop("disabled", false);
		});
}
/*----------------------------------------------------------------------------------------*/
function submitSaveUpdateUpload(ilaka) {
	if (confirm(msg_are_you_sure_to_perform_this_action) == false) { return false; }
	$("#btnSave").prop("disabled", true);
	$("#btnC").prop("disabled", true);
	//var formData = $("#frm").serialize();
	var formData = new FormData($("#frm")[0]);
	/*
	for (var pair of formData.entries()) {
		console.log(pair[0] + ": " + pair[1]);
	}
	*/
	$.ajax({
		url: ilaka + "Save",
		type: "POST",
		data: formData,
		processData: false,
		contentType: false,
		headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
		success: function (data) {
			if (data.status === "success") {

				var table = $("#tblData").DataTable();

				table.ajax.reload(function () {
					setTimeout(function () {
						if (data.id) {

							var rowIdx = table.rows().eq(0).filter(function (idx) {
								return String(table.row(idx).data().id) === String(data.id);
							});

							if (rowIdx.length) {
								var rowNode = table.row(rowIdx[0]).nodes().to$();
								rowNode.addClass("highlight-row");
								setTimeout(function () {
									rowNode.removeClass("highlight-row");
								}, 2000);
							} else {
								//console.warn("Row not found for id:", data.id);
							}
						}
					}, 100);
				}, false);

				var btnText = $("#btnSave").text().trim();
				if (btnText === "Save" || btnText === "Import" || btnText === "Upload") {
					showSuccess(data.message);
					$("#btnSave").prop("disabled", false);
					$("#btnC").prop("disabled", false);
					var modal = bootstrap.Modal.getInstance(document.getElementById("ModalID"));
					if (modal) modal.hide();
				} else if (btnText === "Update") {
					showSuccess(data.message, "modal-message");
					$("#btnSave").text("Edit");
					$("#btnC").text("Close");
					$("#btnSave").prop("disabled", false);
					$("#btnC").prop("disabled", false);
				}

			} else {
				if (btnText === "Upload") {
					showError(data.message, "modal-error");
					$("#btnUpload").prop("disabled", false);
					$("#btnC").prop("disabled", false);
				} else {
					showError(data.message, "modal-message");
					$("#btnSave").prop("disabled", false);
					$("#btnC").prop("disabled", false);
				}
			}
		},
		error: function (xhr) {
			showError(xhr.responseText, "modal-message");
			$("#btnSave").prop("disabled", false);
			$("#btnC").prop("disabled", false);
		}
	});
}
/*----------------------------------------------------------------------------------------
 * Show Hide
 *---------------------------------------------------------------------------------------*/
function ShowHideCTRL(parm_div, parm_disp) { document.getElementById(parm_div).style.display = parm_disp; }	/*endfunction*/
function showHide_ulr(crtl, div) { if (document.getElementById(crtl).checked == true) { document.getElementById(div).style.display = "block"; } else { document.getElementById(div).style.display = "none"; } }
function ShowHide(ctrl, img) { var divCtrl = document.getElementById(ctrl); var imgCtrl = document.getElementById(img); if (divCtrl.style == "" || divCtrl.style.display == "none") { divCtrl.style.display = "block"; imgCtrl.src = MyImgSrc2.src; } else if (divCtrl.style != "" || divCtrl.style.display == "block") { divCtrl.style.display = "none"; imgCtrl.src = MyImgSrc1.src; } }
/*----------------------------------------------------------------------------------------
 * //check if any check box is selected or not
 *---------------------------------------------------------------------------------------*/
function setupRowCheckboxPersistence(table, selectedIds) { table.on('draw', function () { $('.row-checkbox').each(function () { if (selectedIds.has($(this).val())) { $(this).prop('checked', true); } }); }); }
function doAnyCheckboxSelected(msg = "", DivId = "") {/* Collect all checkboxes with class "row-checkbox"*/const checkboxes = document.querySelectorAll('.row-checkbox:checked'); if (checkboxes.length === 0) { return showError(msg, DivId); } else { return true; } }
/*----------------------------------------------------------------------------------------*/
function checkUncheckControl() {/* Select All*/$('#selectAll').on('click', function () { var checked = this.checked; $('.row-checkbox').prop('checked', checked).trigger('change'); });/* Track selections + sync Select All*/$('#tblData').on('change', '.row-checkbox', function () { var id = $(this).val(); if (this.checked) { selectedIds.add(id); } else { selectedIds.delete(id); } if (!this.checked) { $('#selectAll').prop('checked', false); } else if ($('.row-checkbox:checked').length === $('.row-checkbox').length) { $('#selectAll').prop('checked', true); } }); /* ? Initialization after table draw */table.on('draw', function () {/* Add any pre-checked boxes into selectedIds*/$('.row-checkbox:checked').each(function () { selectedIds.add($(this).val()); }); if ($('.row-checkbox:checked').length === $('.row-checkbox').length && $('.row-checkbox').length > 0) { $('#selectAll').prop('checked', true); } else { $('#selectAll').prop('checked', false); } }); }
function CheckAll_ulr(hname, chkname, chkall) { var jj = document.getElementById(hname).value; StrVal = ""; if (jj == "") { jj = 0; } if (document.getElementById(chkall).checked == true) { for (i = 1; i <= jj; i++) { document.getElementById(chkname + i).checked = true; } } else { for (i = 1; i <= jj; i++) { document.getElementById(chkname + i).checked = false; } } }
function CheckOne_ulr(hname, chkname, chkall) { var counter = 0; var jj = document.getElementById(hname).value; if (jj == "") { jj = 0; } for (i = 1; i <= jj; i++) { if (document.getElementById(chkname + i).checked == true) { counter++; } } if (counter == Number(jj)) { document.getElementById(chkall).checked = true; } else { document.getElementById(chkall).checked = false; } }
/********************************* Check All check box */
//function CheckAll_Grp() { var jj = document.frm.HRecCount.value; StrVal = ""; if (jj == "") { jj = 0; } for (i = 1; i <= jj; i++) { StrVal = eval("document.frm.chk" + i); if (StrVal == "undefined" || StrVal == null || StrVal == "") {/* do nothing*/ } else { if (document.frm.chkall.checked == true) { StrVal.checked = true; } else { StrVal.checked = false; } } } }
/********************************* Check individual*/
//function IndividualCheck_Grp() { var counter = 0; var jj = document.frm.HRecCount.value; StrVal = ""; if (jj == "") { jj = 0; } for (i = 1; i <= jj; i++) { StrVal = eval("document.frm.chk" + i); if (StrVal == "undefined" || StrVal == null || StrVal == "") { counter++; } else { if (StrVal.checked == true) { counter++; } } } if (counter == Number(jj)) { document.frm.chkall.checked = true; } else { document.frm.chkall.checked = false; } }
/********************************* Check all*/
//function CheckAll() { var jj = document.frm.HRecCount.value; StrVal = ""; if (jj == "") { jj = 0; } for (i = 0; i < jj; i++) { if (jj == 1) { StrVal = eval("document.frm.chk"); } else { StrVal = eval("document.frm.chk[" + i + "]"); } if (document.frm.chkall.checked == true) { StrVal.checked = true; document.frm.selRecCount.value = jj; } else { StrVal.checked = false; document.frm.selRecCount.value = 0; } } }
/********************************* Check individual */
//function IndividualCheck() { var counter = 0; var jj = document.frm.HRecCount.value; if (jj == "") { jj = 0; } for (i = 0; i < jj; i++) { if (jj == 1) { StrVal = eval("document.frm.chk"); } else { StrVal = eval("document.frm.chk[" + i + "]"); } if (StrVal.checked == true) { counter++; } } document.frm.selRecCount.value = counter; if (counter == Number(jj)) { document.frm.chkall.checked = true; } else { document.frm.chkall.checked = false; } }
/**----------------------------------------------------------------------------------------
 * CLEAR ALL CONTROLS
 *----------------------------------------------------------------------------------------*/
function clearControls(formId, requiredControls) {
	// Call the function with your form id and required controls
	/*
	clearControls("frm", [
		{ id: "username", type: "text" },
		{ id: "email", type: "email" },
		{ id: "password", type: "password" }
	]);
	*/
	// Get the form element
	const form = document.getElementById(formId);
	if (!form) return;

	// Clear all inputs, selects, and textareas inside the form
	form.querySelectorAll("input, select, textarea").forEach(ctrl => {
		// Skip buttons and submit/reset inputs
		if (ctrl.type === "button" || ctrl.type === "submit" || ctrl.type === "reset") {
			return;
		}
		if (ctrl.type === "checkbox" || ctrl.type === "radio") {
			ctrl.checked = false;
		} else {
			ctrl.value = "";
		}
	});
	// Clear divs and spans (reset text content)
	container.querySelectorAll("div, span").forEach(el => {
		el.textContent = "";
	});
	// Attach event listeners to required controls
	// requiredControls should be an array of objects: [{ id: "username", type: "text" }, { id: "email", type: "email" }]
	requiredControls.forEach(rc => {
		const element = document.getElementById(rc.id);
		if (element) {
			element.addEventListener("blur", function () {
				let val = element.value || element.textContent;
				if (!val.trim()) {
					//console.warn(`Required ${rc.type} control '${rc.id}' is empty`);
					element.style.borderColor = "red"; // simple visual feedback
				} else {
					element.style.borderColor = ""; // reset
				}
			});
		}
	});
}
/*----------------------------------------------------------------------------------------
 * Submit only : dynamic page
 *---------------------------------------------------------------------------------------*/
function postdata(post_page) { document.frm.method = "post"; document.frm.action = post_page; document.frm.submit(); }
function enableDisableButton(btnStatus, btnName, btnText = "") { const btn = document.getElementById(btnName); btn.disabled = btnStatus; /* Enable/disable the button*/if (btnText != "") { btn.textContent = btnText;  /*optional: change text*/ } }
/*----------------------------------------------------------------------------------------
 * Check number
 *---------------------------------------------------------------------------------------*/
function OnlyNumeric(valDel, msg_ctrl) { var ctrl = document.getElementById(valDel); if (isNaN(ctrl.value) === true) { showError(msg_enter_only_numeric_value, msg_ctrl); ctrl.value = 0; ctrl.focus(); } }
function OnlyNumericCtrl(ctrl, msg_ctrl) {var value = $(ctrl).val();if (isNaN(value)) {showError(msg_enter_only_numeric_value, msg_ctrl);$(ctrl).val(0).focus();}
}
/*----------------------------------------------------------------------------------------*/
function checkNumber() { var intNumber, strError, i; var ValidChars, CountValidChars; ValidChars = "0123456789"; intNumber = checkNumber.arguments[0]; CountValidChars = 0; if (intNumber.length >= 1) { for (i = 0; i < intNumber.length; i++) { CountValidChars = 0; for (j = 0; j < ValidChars.length; j++) { if (intNumber.charAt(i) == ValidChars.charAt(j)) { CountValidChars++; } } if (CountValidChars == 0) { strError = 0; break; } } } return strError; }
/*----------------------------------------------------------------------------------------*/
function checkAZ09() { var intNumber, strError, i; var ValidChars, CountValidChars; ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; intNumber = checkAZ09.arguments[0]; CountValidChars = 0; if (intNumber.length >= 1) { for (i = 0; i < intNumber.length; i++) { CountValidChars = 0; for (j = 0; j < ValidChars.length; j++) { if (intNumber.charAt(i) == ValidChars.charAt(j)) { CountValidChars++; } } if (CountValidChars == 0) { strError = 0; break; } } } return strError; }
/*----------------------------------------------------------------------------------------
 * 
 *---------------------------------------------------------------------------------------*/
function isValidEmail(em) {/*Regex for basic email validation*/var pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; return pattern.test(em); }
/*----------------------------------------------------------------------------------------
 * Check username validation, if not email
 * 2023-Aug-09
 *---------------------------------------------------------------------------------------*/
function checkUserName(ctl_user) { document.getElementById("username_validity").textContent = validateUsername(document.getElementById(ctl_user).value); }
function validateUsername(username) {/*Try to out put what is invalid given and what is valid not given*/if (username == "") { return "" }/* Check for white space*/if (!/^\S*$/.test(username)) { return msg_username_have_whitespace; }/* Check for Unicode*/if (/[^\u0000-\u007F]/.test(username)) { return msg_username_have_unicode; } if (!/^[a-zA-Z0-9@\-._]+$/.test(username)) { return msg_username_have_invalid_chars; } if (!/^.{4,50}$/.test(username)) { return msg_username_have_invalid_len; } return ""; }
/*----------------------------------------------------------------------------------------
 * Check password validation
 * 2023-Aug-09
 *---------------------------------------------------------------------------------------*/
function checkPassword(ctl_pwd) { document.getElementById("pwd_validity").textContent = validatePassword(document.getElementById(ctl_pwd).value); }
function checkCPassword(ctl_pwd, ctl_cpwd) { document.getElementById("pwd_con_validity").textContent = validateConfirmPassword(ctl_pwd, ctl_cpwd); }
function validatePassword(pwd) {/*var passwordRegExp = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$/; //var passwordRegExp = /^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[a-zA-Z!#$%&? "])[a-zA-Z0-9!#$%&?]{8,20}$/	//let regularExpression = /^(\S)(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])(?=.*[~`!@#$%^&*()--+={}\[\]|\\:;"'<>,.?/_?])[a-zA-Z0-9~`!@#$%^&*()--+={}\[\]|\\:;"'<>,.?/_?]{10,16}$/;*/	/*Try to out put what is invalid given and what is valid not given*/if (pwd == "") { return "" }/* Check for white space*/if (!/^\S*$/.test(pwd)) { return msg_password_have_space; }/* Check for Unicode*/if (/[^\u0000-\u007F]/.test(pwd)) { return msg_password_have_unicode; }/* Check for at least one uppercase letter*/if (!/[A-Z]/.test(pwd)) { return msg_password_have_no_ucase; }/* Check for at least one lowercase letter*/if (!/[a-z]/.test(pwd)) { return msg_password_have_no_lcase; }/* Check for at least one Digit*/if (!/[0-9]/.test(pwd)) { return msg_password_have_no_digit; }/*Check for at least one special character*/if (!/[~`!@#$%^&*()--+={}\[\]|\\:;\"\'<>,.?/_?]/.test(pwd)) { return msg_password_have_no_spc; } if (!/^.{8,20}$/.test(pwd)) { return msg_password_have_no_length; } return ''; }
function validateConfirmPassword(ctrl1, ctrl2) { if (document.getElementById(ctrl1).value != document.getElementById(ctrl2).value) { return msg_new_cpwd_not_same; } else { return ""; } }
function CheckPasswordStrengthCheck(password) {/*Regular Expressions.*/var regex = new Array(); regex.push("[A-Z]"); /*Uppercase Alphabet.*/regex.push("[a-z]"); /*Lowercase Alphabet.*/regex.push("[0-9]"); /*Digit.*/regex.push("[~`!@#$%^&*()--+={}\[\]|\\:;\"\'<>,.?/_?]"); /*Special Character.*/var passed = 0;/*Validate for each Regular Expression.*/for (var i = 0; i < regex.length; i++) { if (new RegExp(regex[i]).test(password)) { passed++; } }/*Validate for length of Password.*/if (passed > 2 && password.length > 8) { passed++; } return passed; }
function CheckPasswordStrength(password) { var password_strength_text = document.getElementById("password_strength_text"); var password_strength_color = document.getElementById("password_strength_color");/*TextBox left blank.*/	if (password.length == 0) { password_strength_text.textContent = ""; password_strength_color.style.background = "none"; return; } var passed = CheckPasswordStrengthCheck(password);/*Display status.*/var color = ""; var strength = ""; switch (passed) { case 0: case 1: strength = "Weak"; color = "#DF0000"; break; case 2: strength = "Good"; color = "#E16D04"; break; case 3: case 4: strength = "Strong"; color = "#14C92B"; break; case 5: strength = "Very Strong"; color = "#008000"; break; }password_strength_text.textContent = strength; password_strength_text.style.color = color; password_strength_color.style.background = color; }
/*----------------------------------------------------------------------------------------
 * Check pin code validation
 * 2023-Aug-12
 *---------------------------------------------------------------------------------------*/
function checkPin(ctl_pin) { document.getElementById("pin_validity").textContent = validatePinCode(document.getElementById(ctl_pin).value); }
function checkCPin(ctl_pin, ctl_cpin) { document.getElementById("pin_con_validity").textContent = validateConfirmPin(ctl_pin, ctl_cpin); }
function validatePinCode(pin) {	/* Check for 6 Digits numbers*/	if (!/^\d{6}$/.test(pin)) { return msg_pin_code_should_have; } return ''; }
function validateConfirmPin(ctrl1, ctrl2) { if (document.getElementById(ctrl1).value != document.getElementById(ctrl2).value) { return msg_enter_same_pin; } else { return ""; } }
function CheckPinStrengthCheck(pin) {/*break all available numbers*/var res = new Array(); var revres = new Array(); var allsame = true; var serial = true; var rserial = true; var passed = 0; res = pin.split('');/*check if all the digits are same like 111111, 000000, 999999*/for (i = 0; i < res.length; i++) { if (res[0] != res[i]) { allsame = false; break; } }/*check if successing order like 12345 */for (i = 0; i < res.length - 1; i++) { if (parseInt(res[i]) + 1 != res[i + 1]) { serial = false; break; } }	/*check if decending order like 54321*/for (i = 0; i < res.length - 1; i++) { if (res[i] - 1 != res[i + 1]) { rserial = false; break; } } if (allsame == true || serial == true || rserial == true) {/*passed++;*/ } else { passed++;/*check if any one reapeat */var outputArray = []; outputArray = Array.from(new Set(res)); if (outputArray.length > 3 || outputArray.length < 5) { passed++; } if (outputArray.length > 4) { passed++; } } if (pin.length > 5) { passed++; } /*Validate for length of PIN.*/return passed; }
function CheckPinStrength(pin) { var pin_strength_text = document.getElementById("pin_strength_text"); var pin_strength_color = document.getElementById("pin_strength_color"); if (pin.length == 0) { pin_strength_text.textContent = ""; pin_strength_color.style.background = "none"; return; } if (checkNumber(pin) == 0) { pin_strength_text.textContent = "Invalid"; pin_strength_text.style.color = "#DF0000"; pin_strength_color.style.background = "#DF0000"; return; } var passed = CheckPinStrengthCheck(pin); var color = ""; var strength = ""; switch (passed) { case 0: case 1: strength = "Weak"; color = "#DF0000"; break; case 2: strength = "Good"; color = "#E16D04"; break; case 3: strength = "Strong"; color = "#14C92B"; break; case 4: strength = "Very Strong"; color = "#008000"; break; }pin_strength_text.textContent = strength; pin_strength_text.style.color = color; pin_strength_color.style.background = color; }
function getRandomNumber(min, max) { return Math.floor(Math.random() * (max - min + 1)) + min; }
//----------------------------------------------------------------------------------------
/*----------------------------------------------------------------------------------------
 * Countdown Manager JavaScript
 *---------------------------------------------------------------------------------------*/
function startSessionCountdown(parm_time, parm_page) {var realSessionValue = parseInt(parm_time) * 60; /* seconds */var sessionExpiration = new Date().getTime() + (realSessionValue * 1000); localStorage.setItem('sessionExpiration', sessionExpiration); var countDownElement = document.getElementById("sCountDown"); function updateSessionCountdown() { var currentTime = new Date().getTime(); var timeLeft = Math.max(0, sessionExpiration - currentTime); var minutes = Math.floor(timeLeft / 60000); var seconds = Math.floor((timeLeft % 60000) / 1000); countDownElement.textContent = `Session time out in : ${minutes} Minute(s) ${seconds} Second(s)`; if (timeLeft === 0) { clearInterval(sessionCountdownInterval); window.location.href = '/Account/Logout'; } }function resetSession() {fetch('/Account/KeepAlive',{method: 'POST',headers: {'Content-Type': 'application/json',/*'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value*/}}).then(response => {if (!response.ok) {console.error("KeepAlive failed:", response.status);} else {/*console.log("Session refreshed");*/}}).catch(error => console.error("Error calling KeepAlive:", error));/* Reset expiration timestamp*/sessionExpiration = new Date().getTime() + (realSessionValue * 1000);}/*Initial update*/updateSessionCountdown();/* Countdown tick*/var sessionCountdownInterval = setInterval(updateSessionCountdown, 1000);/* Debounced activity handler  | wait 1s before ping*/let activityTimeout;function handleActivity(){clearTimeout(activityTimeout); activityTimeout = setTimeout(resetSession, 1000);}document.addEventListener('click', handleActivity);document.addEventListener('keypress', handleActivity);}		
/*----------------------------------------------------------------------------------------
 * Clear and hide message box
 *----------------------------------------------------------------------------------------*/
function do_empty(msg_ctrl) { if (msg_ctrl == null || msg_ctrl == 'undefined' || msg_ctrl == '') { var el = document.getElementById('message'); } else { var el = document.getElementById(msg_ctrl); } var style = window.getComputedStyle(el); if (style.display !== "none" && style.visibility !== "hidden" && style.opacity !== "0") {/*Element is visible*/el.style.display = "none"; /* hide it */el.textContent = ""; } return; }
/*----------------------------------------------------------------------------------------
 * Edit form from view
 *----------------------------------------------------------------------------------------*/
function editinformation(val) { document.getElementById("header").textContent = val; document.getElementById("div_edit").style.display = "block"; document.getElementById("div_view").style.display = "none"; do_empty(); }
/*----------------------------------------------------------------------------------------
 * Cancel Edit form from view
 *---------------------------------------------------------------------------------------*/
function editcancel(val) { document.getElementById("header").textContent = val; document.getElementById("div_edit").style.display = "none"; document.getElementById("div_view").style.display = "block"; do_empty(); }
/*----------------------------------------------------------------------------------------
 * Since : 2007-Jul-01 | CONTROLS VALIDATION | 
 *---------------------------------------------------------------------------------------*/
function chkSpace(txtfield) { var i, j, txtval; j = 0; txtfield = document.getElementById(txtfield); if (txtfield) { txtval = txtfield.value; for (i = 0; i < txtval.length; i++) { j++; if (txtval.substr(i, 1) != " ") { break; } } txtval = txtval.substr(j - 1, txtval.length); if (txtval == " ") { txtval = ""; txtfield.value = txtval; } return (txtval); } else {/*alert(txtfield + " Not found");*/ return ""; } }
function checkInput(type, cname, msg, msg_ctrl) { var ctrl = ""; flag = true; if (msg_ctrl == null || msg_ctrl == "undefined" || msg_ctrl == "") { var y = document.getElementById("message"); } else { var y = document.getElementById(msg_ctrl); }/*endif*/y.className = "displaynone"; y.textContent = ""; if (type == "txt") { if (chkSpace(cname) == "") { flag = false; }/*endif*/ } else if (type == "file") { var ctrl = document.getElementById(cname); if (ctrl.value == "") { flag = false; }/*endif*/ } else if (type == "cmb") { var ctrl = document.getElementById(cname).options[document.getElementById(cname).selectedIndex]; if (ctrl.value == "" || ctrl.value == "session_off") { flag = false; }/*endif*/ } else if (type == "email") { var ctrl = document.getElementById(cname); if (isValidEmail(ctrl.value) == false) { flag = false; }/*endif*/ }/*endif*/if (flag == false) { return do_error(cname, msg, msg_ctrl); } else { return true; } /*endif*/ }/*endfunction*/
/*----------------------------------------------------------------------------------------
 * Since : 2007-Jul-01 | RETURN FOR SUBMITTING |
 *---------------------------------------------------------------------------------------*/
function do_error(cname, msg, msg_ctrl) { if (msg_ctrl == null || msg_ctrl == "undefined" || msg_ctrl == "") { var y = document.getElementById("message"); } else { var y = document.getElementById(msg_ctrl); }/*endif*/	y.textContent = ""; if (cname == null || cname == "undefined" || cname == "") { /*void*/ } else { if (document.getElementById(cname).type != "hidden") { document.getElementById(cname).focus(); }/*endif*/ }/*endif*/	y.style.display = "block"; y.className = "error"; y.textContent = msg; return false; }/*endfunction*/
//----------------------------------------------------------------------------------------
function validateFundSource(fund_source) {
	var strError = "";

	if (fund_source != "") {
		var chk_das1 = fund_source.charAt(3);
		var chk_das2 = fund_source.charAt(12);

		if (fund_source.length == 17 || fund_source.length == 21) {
			if (chk_das1 != "-" && chk_das2 != "-")
				return false;
			else {
				var chk_split = fund_source.split("-");
				var chk_len0 = chk_split[0];
				var chk_len1 = chk_split[1];
				var chk_len2 = chk_split[2];

				if (chk_len0.length == 3 && chk_len1.length == 8 && (chk_len2.length == 4 || chk_len2.length == 8)) {
					/*if(checkNumber(chk_len0) == 0 || checkNumber(chk_len2) == 0)*/ //Update on 2015-12-23 as Fund source will sometime come alpha numeric
					if (checkNumber(chk_len0) == 0) {
						return false
					}
					else {
						if (checkAZ09(chk_len1) == 0) { return false; }
					}
				}
				else {
					return false;
				}
			}
		}
		else {
			return false;
		}
	}
	return true;
}
/*----------------------------------------------------------------------------------------
 * 
 *---------------------------------------------------------------------------------------*/
function getFileType(filename, dpath, id) {
	file_format = filename.split(".").pop().toLowerCase();
	var str_download = "";

	if (file_format == "doc" || file_format == "docx") { file_format = "docx.png"; }
	else if (file_format == "xls" || file_format == "xlsx") { file_format = "xlsx.png"; }
	else if (file_format == "ppt" || file_format == "pptx") { file_format = "pptx.png"; }
	else if (file_format == "jpg" || file_format == "jpeg") { file_format = "jpg.png"; }
	else if (file_format == "png") { file_format = "png.png"; }
	else if (file_format == "bmp") { file_format = "bmp.png"; }
	else if (file_format == "pdf") { file_format = "pdf.png"; }
	else { file_format = "ukn.png" }

	if (file_format != null && file_format != "") {
		if (file_format == "ukn.png") {
			str_download = `<img src="${path}images/ukn.png" title="" width="30" height="40" border="0">`;
		} else {
			str_download = `
				<a href = "${dpath}/${id}" target="_blank">
					<img src="${path}images/${file_format}" title="" width="30" height="40" border="0">
				</a>`;
		}
	}
	return str_download
}
function chk_load_document_templates() { if (document.frm.chk.checked) { document.getElementById("hid_doc_also").value = "Y" } else { document.getElementById("hid_doc_also").value = "N" } }

/*----------------------------------------------------------------------------------------
 * Since : 2026-06-24
 *---------------------------------------------------------------------------------------*/
function formatDateToISO(dateStr) {
	// assumes input like "06/24/2026"
	// parts[0] = month, parts[1] = day, parts[2] = year
	if (!dateStr) {
		return null; // or return ""; depending on what you want
	}
	const parts = dateStr.split("/");
	return `${parts[2]}-${parts[0].padStart(2, "0")}-${parts[1].padStart(2, "0")}`;
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function clearContainer(containerId) {
	const container = document.getElementById(containerId);
	if (container) {
		container.textContent = "";   // clears all text and child nodes
		// or: container.innerHTML = ""; // alternative
		// or: while (container.firstChild) { container.removeChild(container.firstChild); }
	}
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function renderModalButtons(containerId, mode) {
	const container = document.getElementById(containerId);
	container.textContent = "";
	const saveBtn = document.createElement("button");
	saveBtn.type = "button";
	saveBtn.name = "btnSave";
	saveBtn.id = "btnSave";
	saveBtn.className = "button btn-primary";
	if (mode === "Upload") {
		saveBtn.textContent = "Upload";
	} else {
		saveBtn.textContent = "Save";
	}
	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";
	container.append(saveBtn, closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function renderModalCloseButton(containerId) {
	const container = document.getElementById(containerId);
	container.textContent = "";
	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";
	container.append(closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function renderModalExportButtons(containerId) {
	const container = document.getElementById(containerId);
	container.textContent = "";

	const pdfBtn = document.createElement("button");
	pdfBtn.type = "button";
	pdfBtn.name = "btnPDF";
	pdfBtn.id = "btnPDF";
	pdfBtn.className = "button btn-primary";
	pdfBtn.textContent = "Export to PDF";

	const xlsBtn = document.createElement("button");
	xlsBtn.type = "button";
	xlsBtn.name = "btnExcel";
	xlsBtn.id = "btnExcel";
	xlsBtn.className = "button btn-primary";
	xlsBtn.textContent = "Export to Excel";

	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";


	container.append(pdfBtn, xlsBtn, closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function renderModalExportButton(containerId) {
	const container = document.getElementById(containerId);
	container.textContent = "";

	const Btn = document.createElement("button");
	Btn.type = "button";
	Btn.name = "btnExport";
	Btn.id = "btnExport";
	Btn.className = "button btn-primary";
	Btn.textContent = "Export";

	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";

	container.append(Btn, closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-27
 *---------------------------------------------------------------------------------------*/
function renderModalSubmitButtons(btnSave, btnSubmit) {
const container = document.getElementById("modal-button-list");
	container.textContent = "";
	if (btnSave !== "") {
		const Btn1 = document.createElement("button");
		Btn1.type = "button";
		Btn1.name = "btnSave";
		Btn1.id = "btnSave";
		Btn1.className = "button btn-primary";
		Btn1.textContent = btnSave;
		container.append(Btn1);
	}
	if (btnSubmit !== "") {
		const Btn2 = document.createElement("button");
		Btn2.type = "button";
		Btn2.name = "btnSubmit";
		Btn2.id = "btnSubmit";
		Btn2.className = "button btn-primary";
		Btn2.textContent = btnSubmit;
		container.append(Btn2);
	}

	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";

	container.append(closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-14
 * In case if URL need to check and flag dafe or not
 *---------------------------------------------------------------------------------------*/
function renderModalImportButtons(containerId, parm) {
	const container = document.getElementById(containerId);
	container.textContent = "";
	const saveBtn = document.createElement("button");
	saveBtn.type = "button";
	saveBtn.name = "btnSave";
	saveBtn.id = "btnSave";
	saveBtn.className = "button btn-primary";
	saveBtn.textContent = parm;
	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";
	container.append(saveBtn, closeBtn);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-06-28
 * In case if URL need to check and flag dafe or not
 *---------------------------------------------------------------------------------------*/
function safeRedirect(url) {
	try {
		const parsed = new URL(url, window.location.origin);
		// Only allow same-origin redirects
		if (parsed.origin === window.location.origin) {
			window.location.href = parsed.href;
		} else {
			console.error("Blocked unsafe redirect:", url);
		}
	} catch {
		console.error("Invalid URL:", url);
	}
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * 
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	const helpLink = document.getElementById("helpLink");
	const divHelp = document.getElementById("divHelp");
	if (helpLink)
	{
		helpLink.addEventListener("click", function (e) {
			e.preventDefault(); // prevent "#" navigation
			showHelp('/');
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * To show hide Module > menu
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	document.querySelectorAll(".menu-t").forEach(link => {
		if (link) {
			link.addEventListener("click", function (e) {
				e.preventDefault();
				const targetId = this.dataset.target;
				const imgId = this.dataset.img;
				ShowHide(targetId, imgId);
			});
		}
	});
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * refresh captcha
 *---------------------------------------------------------------------------------------*/
function refreshCaptcha() { var captchaImage = document.getElementById("cvscaptcha"); captchaImage.src = "/Account/GenerateCaptcha?t=" + new Date().getTime(); }
document.addEventListener("DOMContentLoaded", function () {
	const refreshLink = document.getElementById("refreshCaptcha");
	const captchaImg = document.getElementById("cvscaptcha");
	if (refreshLink) { 
		refreshLink.addEventListener("click", function (e) {
			e.preventDefault();
			refreshCaptcha()
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * 
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	const btnreturnlogin = document.getElementById("btnreturnlogin");
	if (btnreturnlogin) {
		btnreturnlogin.addEventListener("click", function (e) {
			e.preventDefault();
			top.location.href = btnreturnlogin.dataset.url;
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * 
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	const btnOpenEdit = document.getElementById("btnOpenEdit");
	if (btnOpenEdit) {
		btnOpenEdit.addEventListener("click", function (e) {
			e.preventDefault();
			editinformation(btnOpenEdit.dataset.update);
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * 
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	const btnCancelEdit = document.getElementById("btnCancelEdit");
	if (btnCancelEdit) {
		btnCancelEdit.addEventListener("click", function (e) {
			e.preventDefault();
			editcancel(btnOpenEdit.dataset.update);
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * 
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () {
	const btnAdd = document.getElementById("btnAdd");
	if (btnAdd) {
		btnAdd.addEventListener("click", function (e) {
			e.preventDefault();
			e.stopPropagation();
			add(e);
		});
	}
});
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 * delete button call
 *---------------------------------------------------------------------------------------*/
document.addEventListener("DOMContentLoaded", function () { const btnDelete = document.getElementById("btnDelete"); if (btnDelete) { btnDelete.addEventListener("click", function (e) { e.preventDefault(); delData(document.getElementById('ilaka').value); }); } });
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-01
 *---------------------------------------------------------------------------------------*/
function GetEmployeeListStatusWise(ilaka, ctrl, status)
{
	$.ajax({
		url: ilaka + 'ByStatus',
		type: 'GET',
		data: { status: status },
		success: function (data) {
			$("#" + ctrl).replaceWith(data);
			$("#" + ctrl).val("");
		}
	});
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-08-05
 *---------------------------------------------------------------------------------------*/
function ImportFile()
{
	$("#modal-message").text("").hide();
	$("#ModalBody").html("");
	$.get(ilaka + "Import", function (data)
	{
		$("#ModalBody").html(data);
		$("#ModalID .modal-title").text("Import");
		var modal = new bootstrap.Modal(document.getElementById("ModalID"));
		modal.show();
	}).fail(function (xhr) { showError("Error occured while loading record(s): " + xhr.responseText); });
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-19
 *---------------------------------------------------------------------------------------*/
function showReport(ilaka, payload) {
	$("#ModalBody").html("");
	$.ajax({
		type: "POST",
		url: ilaka,
		data: JSON.stringify(payload),
		contentType: "application/json; charset=utf-8",
		dataType: "html",
		headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
		success: function(data) {
			$("#ModalBody").html(data);
			$("#ModalID .modal-title").text("Report");
			var modal = new bootstrap.Modal(document.getElementById("ModalID"));
			modal.show();
		}
		}).fail(function (xhr) {
			showError("Error occurred while loading record(s): " + xhr.responseText);
		});
}
/********************************* Calculate hours by two date and time  */
function timeDiff(start, end) {
	var startDate = new Date(start);
	var endDate = new Date(end);

	var diff = endDate.getTime() - startDate.getTime();
	var minutes = Math.floor(diff / 1000 / 60) / 60;
	return minutes.toFixed(2);

	//var hours = Math.floor(diff / 1000 / 60 / 60);
	//diff -= hours * 1000 * 60 * 60;    
	//return (hours <= 9 ? "0" : "") + hours + ":" + (minutes <= 9 ? "0" : "") + minutes;
}
/*********************************  day diff */
function getDiffDays(date_from, date_to) {
	var date1 = new Date(date_from);
	var date2 = new Date(date_to);

	var timeDiff = Math.abs(date2.getTime() - date1.getTime());
	var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));

	return diffDays
}

/*********************************/
function isStartDateGreater(start, end) {
	if (!start || !end) {
		return false; // or `null`/throw, depending on what you want for missing input
	}

	var startDate = new Date(start);
	var endDate = new Date(end);

	if (isNaN(startDate) || isNaN(endDate)) {
		return false; // or throw new Error("Invalid date input")
	}

	return startDate > endDate; // real boolean, no need for if/else
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-07-19
 *---------------------------------------------------------------------------------------*/
function parseDate(str) {
	// expects MM/dd/yyyy
	const parts = str.split('/');
	return new Date(parts[2], parts[0] - 1, parts[1]);
}
/*----------------------------------------------------------------------------------------
* 
*---------------------------------------------------------------------------------------*/
function addDays(date, days) {
	const result = new Date(date);
	result.setDate(result.getDate() + days);
	return result;
}
/*----------------------------------------------------------------------------------------
* 
*---------------------------------------------------------------------------------------*/
function calculateLeaveHours() {
	const workingHrsEl = document.getElementById("workingHoursDays");
	const leaveTypeEl = document.getElementById("leave_type_id");
	const fromDateEl = document.getElementById("leave_from_date");
	const toDateEl = document.getElementById("leave_to_date");
	var leavedays = 0;
	if (!workingHrsEl || !leaveTypeEl || !fromDateEl || !toDateEl) {
		console.warn("One or more elements not found yet");
		return;
	}

	const workingHrs = parseFloat(workingHrsEl.value);
	const leaveTypeId = leaveTypeEl.value;
	const fromDateStr = fromDateEl.value;
	const toDateStr = toDateEl.value;

	if (!leaveTypeId || !fromDateStr || !toDateStr) return;

	const fromDate = parseDate(fromDateStr);
	const toDate = parseDate(toDateStr);

	if (fromDate > toDate) {
		fromDateEl.value = "";
		toDateEl.value = "";
		document.getElementById("leave_in_hrs").value = "0";
		document.getElementById("leaveindays").innerText = "0.0";
		return;
	}

	const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
	let cntWeekend = 0, cntHoliday = 0, cntDayoff = 0, cntWorkday = 0;

	let current = new Date(fromDate);
	while (current <= toDate) {
		cntWorkday++;
		const dayName = days[current.getDay()];

		if (dayName === "Saturday" || dayName === "Sunday") {
			cntWeekend++;
		} else {
			if ((window.holidays || []).some(h => h.getTime() === current.getTime())) cntHoliday++;
			if ((window.dayOffs || []).some(d => d.getTime() === current.getTime())) cntDayoff++;
		}
		current = addDays(current, 1);
	}

	// Base leave days
	let leaveInDays = ["12", "13", "14"].includes(leaveTypeId)
		? cntWorkday
		: cntWorkday - (cntWeekend + cntHoliday + cntDayoff);

	// Apply half-day factor consistently
	let halfFactor = ["2", "4", "6", "10"].includes(leaveTypeId) ? 0.5 : 1;
	leaveInDays = leaveInDays * halfFactor;
	let leaveInHrs = leaveInDays * workingHrs;

	document.getElementById("leave_in_hrs").value = leaveInHrs;
	document.getElementById("leaveindays").innerText = leaveInDays.toFixed(1);
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-08-06
 *---------------------------------------------------------------------------------------*/
function updateDataWithoutJSON(parm) {
	if (confirm(msg_are_you_sure_to_perform_this_action)) {
		var payload = {
			mode: "updateDataNoChk"
		};
		var ids = Array.from(selectedIds); // e.g. ["12","45","78"]
		if (ids.length > 0) {
			payload.selectedIds = [...selectedIds];
		}
		if (typeof collectExtraFields === "function") {
			var extraFields = collectExtraFields();
			if (extraFields && extraFields.length > 0) {
				payload.Fields = extraFields;
			}
		}
		$.ajax({
			type: "POST",
			url: parm,
			data: JSON.stringify(payload),
			contentType: "application/json; charset=utf-8", // ? tell server it's JSON
			dataType: "json",                     // expect JSON back
			headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
			success: function (data) {
				if (data.status) {   // match your controller return { status = true/false }
					if (data.status === "success") { showSuccess(data.message); } else { showError("Error: " + data.message); }
					$('#tblData').DataTable().ajax.reload(null, false);
				} else {
					showError("Error: " + data.message);
				}
			},
			error: function (xhr, status, error) {
				showError("Request failed: " + error);
			}
		});

	}
}
/*----------------------------------------------------------------------------------------
 * Since : 2026-08-04
 *---------------------------------------------------------------------------------------*/
function renderModalDiscardButton(containerId) {


	const container = document.getElementById(containerId);
	container.textContent = "";

	const saveBtn = document.createElement("button");
	saveBtn.type = "button";
	saveBtn.name = "btnSave";
	saveBtn.id = "btnSave";
	saveBtn.className = "button btn-primary";
	saveBtn.textContent = "Save";

	const discardBtn = document.createElement("button");
	discardBtn.type = "button";
	discardBtn.name = "btnDiscard";
	discardBtn.id = "btnDiscard";
	discardBtn.className = "button btn-primary";
	discardBtn.textContent = "Discard";
	container.append(saveBtn, discardBtn);

	const closeBtn = document.createElement("button");
	closeBtn.type = "button";
	closeBtn.name = "btnC";
	closeBtn.id = "btnC";
	closeBtn.className = "button btn-secondary";
	closeBtn.textContent = "Close";
	container.append(saveBtn, discardBtn, closeBtn);
}
/*----------------------------------------------------------------------------------------
*
*---------------------------------------------------------------------------------------*/
