    function check_dashain_new(is_dashain_already, basic_salary, ival, rdo_chk) {
        var strError = "";
        var dashain_amount = 0;
        if (is_dashain_already === "Y") {
            document.getElementsByName("dashain_a" + ival).value = 0;
            //eval( "document.frm.dashain_a_h" + ival).value	= 0;
            strError += "\n" + msg_pay_you_cant_add_dashain_bonus_twice;
            alert(StrInitialMsg + strError);
        } else {
            var percent_for_tax_add = rdo_selected_percent(ival);

            if (percent_for_tax_add === "") {
                //alert(msg_pay_chk_dashain_select_percent_add_for_dashain);
                document.getElementsByName("is_dashain" + ival).checked = false;
                return false;
            }

            dashain_amount = cal_dashain_with_tax(is_dashain_already, basic_salary, ival);

            if (document.getElementsByName("is_dashain" + ival).checked) {
                document.getElementsByName("dashain_a" + ival).value = dashain_amount.toFixed(0);
                //eval( "document.frm.dashain_a_h" + ival ).value = dashain_amount.toFixed(0);
            } else {
                document.getElementsByName("dashain_a" + ival).value = 0;
                //eval( "document.frm.dashain_a_h" + ival).value	= 0;
            }
        }

        calculate_employee_salary_new(ival);
    }
    function rdo_selected_percent(ival) {
        var percent_for_tax_add = "";
        var radios = document.getElementsByName("rdo_val" + ival).value;

        // Loop only through the actual number of radio buttons
        /*for (var i = 0; i < radios.length; i++) {
            // Debugging: show the radio element and its value
            // console.log(i + " -- " + radios[i].value);

            if (radios[i].checked) {
                percent_for_tax_add = "r" + i;
                break;
            }
        }*/
        // Get the dropdown element by name
        var ddl = document.getElementsByName("rdo_val" + ival)[0];
        // Get the selected value
        var percent_for_tax_add = ddl.value;
        // Or if you want the text shown to the user:
        var percent_text = ddl.options[ddl.selectedIndex].text;
        //alert("Selected value: " + percent_for_tax_add + "\nSelected text: " + percent_text);

        return percent_for_tax_add;
    }
	function getParseFloatValue(parm_ctrl_name, parm_ctrl_index) {
		var els = document.getElementsByName(parm_ctrl_name + parm_ctrl_index);
		if (els.length === 0) {
			return 0; // no element found
		}

		var ctrl_value = els[0].value;

		// Validate and parse
		if (ctrl_value === "" || isNaN(ctrl_value)) {
			return 0;
		}

		return parseFloat(ctrl_value);
	}
function calculate_employee_salary_new(ival) {
        // Helpers
        function getValue(name) {
            var els = document.getElementsByName(name + ival);
            return els.length > 0 ? parseFloat(els[0].value) || 0 : 0;
        }
        function setValue(name, val) {
            var els = document.getElementsByName(name + ival);
            if (els.length > 0) els[0].value = val;
        }
        function getRawValue(name) {
            var els = document.getElementsByName(name + ival);
            return els.length > 0 ? els[0].value : "";
        }
        function isChecked(name) {
            var els = document.getElementsByName(name + ival);
            return els.length > 0 && els[0].checked;
        }
        function getValueTaxSetting(name) {
            var els = document.getElementsByName(name);
            return els.length > 0 ? parseFloat(els[0].value) || 0 : 0;
        }
        /*
        TAX SETTING VALUES
        */
        var d_0_p = getValueTaxSetting("d_0_p");
        var d_amt_s = getValueTaxSetting("d_amt_s");
        var d_amt_m = getValueTaxSetting("d_amt_m");
        var d_1_p = getValueTaxSetting("d_1_p");
        var d_1_a = getValueTaxSetting("d_1_a");
        var d_2_p = getValueTaxSetting("d_2_p");
        var d_2_a = getValueTaxSetting("d_2_a");
        var d_3_p = getValueTaxSetting("d_3_p");
        var d_3_a_s = getValueTaxSetting("d_3_a_s");
        var d_3_a_m = getValueTaxSetting("d_3_a_m");
        var d_4_p = getValueTaxSetting("d_4_p");
        var d_female_p = getValueTaxSetting("d_female_p");
        var r_max_med_e = getValueTaxSetting("r_max_med_e");
        var d_max_med_a = getValueTaxSetting("d_max_med_a");
        var d_max_med_p = getValueTaxSetting("d_max_med_p");

        var d_max_ins_amt = getValueTaxSetting("d_max_ins_amt");
        var d_max_ins_amt_nl = getValueTaxSetting("d_max_ins_amt_nl");
        var fourth_tax_amount = getValueTaxSetting("fourth_tax_amount");
        var fifth_tax_percent = getValueTaxSetting("fifth_tax_percent");

        var gender_ded = getValue("gender_ded");
        var d_3_a = getValueTaxSetting("d_3_a_m"); //ThirdTaxAmountMarried

        var d_amt = getValue("d_amt");
        /*
        ELIGIBLE COUNTINGS
        */
        var sal_got_count = getValue("sal_got_count");
        var month_diff = getValue("month_diff");
        var count_days_dashain = getValue("count_days_dashain");
        /*
        PREVIOUSLY TAKEN VALUES
        */
        var basic_salary_sum_taken = getValue("basic_salary_sum_taken");
        var pf_a_taken = getValue("pf_a_taken");
        var remote_area_all_taken = getValue("remote_area_all_taken");
        var dashain_amount_taken = getValue("dashain_amount_taken");
        var performance_all_taken = getValue("performance_all_taken");
        var children_edu_all_taken = getValue("children_edu_all_taken");
        var gratuity_taken = getValue("gratuity_taken");
        var gratuity_ded_taken = getValue("gratuity_ded_taken");
        var ssf_taken = getValue("ssf_taken");
        var ssf_ded_taken = getValue("ssf_ded_taken");
        var med_exp_reim_eligible_taken = getValue("med_exp_reim_eligible_taken");
        var med_exp_reim_total_taken = getValue("med_exp_reim_total_taken");
        var medical_deduction_on_tax_taken = getValue("medical_deduction_on_tax_taken");
        var leave_encash_taken = getValue("leave_encash_taken");
        var others_taken = getValue("others_taken");
        var betalibi_d_dud = getValue("betalibi_d_dud");
        var pf_d_dud = getValue("pf_d_dud");
        var sal_got_overtime_sum = getValue("sal_got_overtime_sum");
        var insurance_taken = getValue("insurance_taken");
        var pre_access_tax_taken = getValue("pre_access_tax_taken");
        var tax_dud = getValue("tax_dud");

        var act_basic_salary = getValue("act_basic_salary");
        var act_pf_a = getValue("act_pf_a");
        var act_remote_area_all = getValue("act_remote_area_all");
        var act_pf_d = getValue("act_pf_d");
        var act_gratuity = getValue("act_gratuity_h");
        var act_gratuity_ded = getValue("act_gratuity_ded_h");
        var act_ssf = getValue("act_ssf_h");
        var act_ssf_ded = getValue("act_ssf_ded_h");

        var is_dashain_already = getRawValue("is_dashain_already");

        /*
        CURRENT MONTH BONUS
        */
        var basic_salary = getValue("basic_salary");
        //alert(basic_salary);
        var pf_a = getValue("pf_a");
        var performance_all = getValue("performance_all");
        var insurance = getValue("yearly_insurance");
        var others = getValue("others");
        var children_edu_all = getValue("children_edu_all");
        var gratuity = getValue("gratuity");
        var ssf = getValue("ssf");
        var remote_area_all = getValue("remote_area_all");
        var overtime = getValue("overtime");

        var dud_remote_area_all = getValue("dud_remote_area_all");
        var medical_expense_reimburse_total = getValue("medical_expense_reimburse_total");
        var medical_expense_reimburse_eligible = getValue("medical_expense_reimburse_eligible");

        var leave_encash = getValue("leave_encash");
        var dashain_a = getValue("dashain_a");
        var pf_d = getValue("pf_d");
        var gratuity_ded = getValue("gratuity_ded");
        var ssf_ded = getValue("ssf_ded");
        var insurance_d = getValue("insurance_d");
        var insurance_d_nl = getValue("insurance_d_nl");
        var betalibi_d = getValue("betalibi_d");
        var pre_access_tax = getValue("pre_access_tax");
        var medical_deduction_on_tax = getValue("medical_deduction_on_tax");

        /*
        DEFINING OTHER REQUIRED VARIABLES
        */
        var dashain_amount = 0;
        var minus_sal_got_count_12 = 0;
        var rtr_ded_annual = 0;
        var strTotalGrossSalary = 0;
        var str1l3ofGross = 0;
        var taxable_amt = 0;
        var net_in_hand = 0;

        /*
        CALCULATE DASHAIN WITH TAX
        */
        var isDashainforce = getRawValue("isDashainforce");
        if (isDashainforce === "Y") {
            dashain_amount = getValue("dashain_a");
        } else {
            dashain_amount = cal_dashain_with_tax(is_dashain_already, act_basic_salary, ival);
        }
        if (dashain_amount_taken > 0) { dashain_amount = dashain_amount_taken; }
        //alert(month_diff)
        minus_sal_got_count_12 = parseFloat(month_diff - 1);

        /*
        CALCULATING THE GROSS SALARY
        */
        strTotalGrossSalary += basic_salary_sum_taken + pf_a_taken + gratuity_taken + ssf_taken;

        strTotalGrossSalary += remote_area_all_taken + performance_all_taken + children_edu_all_taken;
        strTotalGrossSalary += med_exp_reim_total_taken + leave_encash_taken;
        strTotalGrossSalary += others_taken + sal_got_overtime_sum + insurance_taken;
        strTotalGrossSalary -= betalibi_d_dud;
        //alert(strTotalGrossSalary + " Suraj");
        strTotalGrossSalary += basic_salary + (act_basic_salary * minus_sal_got_count_12);
            
        strTotalGrossSalary += pf_a + (act_pf_a * minus_sal_got_count_12);
            
        strTotalGrossSalary += gratuity + (act_gratuity * minus_sal_got_count_12);
        strTotalGrossSalary += ssf + (act_ssf * minus_sal_got_count_12);
        strTotalGrossSalary += remote_area_all + (act_remote_area_all * minus_sal_got_count_12);
        strTotalGrossSalary += performance_all + children_edu_all;
        strTotalGrossSalary += medical_expense_reimburse_total + leave_encash;
        strTotalGrossSalary += others + overtime + insurance;
        strTotalGrossSalary -= betalibi_d;
        strTotalGrossSalary += parseFloat(dashain_amount);
        //alert(act_basic_salary + '--' + minus_sal_got_count_12);
        //alert(strTotalGrossSalary + '--' + basic_salary_sum_taken + '--' + pf_a_taken + '--' + gratuity_taken + '--' + ssf_taken + '--' + remote_area_all_taken + '--' + performance_all_taken + '--' + children_edu_all_taken + '--' + med_exp_reim_total_taken + '--' + leave_encash_taken +'--' + others_taken + '--' + sal_got_overtime_sum + '--' + insurance_taken + '--' + betalibi_d_dud + '--' + basic_salary + '--' + (act_basic_salary * minus_sal_got_count_12) + '--' + pf_a + '--' + (act_pf_a * minus_sal_got_count_12) + '--' + gratuity + '--' + (act_gratuity * minus_sal_got_count_12) + '--' + ssf + '--' + (act_ssf * minus_sal_got_count_12) + '--' + remote_area_all + '--' + (act_remote_area_all * minus_sal_got_count_12) + '--' + performance_all + '--' + children_edu_all + '--' + medical_expense_reimburse_total + '--' + leave_encash + '--' + others + '--' + overtime + '--' + insurance + '--' + betalibi_d + '--' + parseFloat(dashain_amount));
        /*
        CALCULATE CIT
        */
        var cit_type = getRawValue("cit_type");
        if (cit_type === "") { cit_type = "F"; }
        var cit_dud = getValue("cit_dud");
        var cit_percent_amount = getValue("cit_percent_amount");
        var gross_cit = 0;

        str1l3ofGross = parseFloat(strTotalGrossSalary / 3).toFixed(2);

        var cit_d = 0;
        if (cit_type === "B") {
            cit_d = basic_salary * cit_percent_amount / 100;
            gross_cit = cit_dud + cit_d + (cit_d * minus_sal_got_count_12);
        }
        else if (cit_type === "F") {
            cit_d = cit_percent_amount;
            gross_cit = cit_dud + cit_d + (cit_percent_amount * minus_sal_got_count_12);
        }
        else if (cit_type === "T") {
            gross_cit = str1l3ofGross - (pf_d_dud + pf_d + (act_pf_d * minus_sal_got_count_12));
            cit_d = (gross_cit - cit_dud) / month_diff;
        }
        setValue("cit_d", cit_d.toFixed(0));
        setValue("cit_d_h", cit_d.toFixed(0));
        setValue("act_cit_d", cit_d.toFixed(0));
        setValue("act_cit_d_h", cit_d.toFixed(0));

        // SSF (PF + GR) + CIT
        rtr_ded_annual = pf_d_dud + pf_d + (act_pf_d * minus_sal_got_count_12) + gross_cit;
        rtr_ded_annual += gratuity_ded_taken + gratuity_ded + (act_gratuity_ded * minus_sal_got_count_12);
        rtr_ded_annual += ssf_ded_taken + ssf_ded + (act_ssf_ded * minus_sal_got_count_12);

        // 5,00,000 OR 1/3 of totalGrossSalary OR SSF(PF+GR)+CIT [whichever is lower]
        if (str1l3ofGross <= rtr_ded_annual) { rtr_ded_annual = str1l3ofGross; }
        if (rtr_ded_annual > 500000) { rtr_ded_annual = 500000; }

        /*
        YEARLY GROSS BEFORE TAX
        */
        //alert(strTotalGrossSalary + " Dipesh")
        strTotalGrossSalary -= rtr_ded_annual;
        strTotalGrossSalary -= insurance_d;
        strTotalGrossSalary -= insurance_d_nl;
        strTotalGrossSalary -= dud_remote_area_all;

        //alert(strTotalGrossSalary);
        setValue("yearly_gross_salary", strTotalGrossSalary.toFixed(0));
        setValue("yearly_gross_salary_h", strTotalGrossSalary.toFixed(0));

        /*
        CALCULATE TAX
        */
        taxable_amt = parseFloat(strTotalGrossSalary);
        var taxable_new_amt = 0;
        var t_0_a = 0, t_1_a = 0, t_2_a = 0, t_3_a = 0, t_4_a = 0, t_5_a = 0;
        var t_a_x = 0;
        //alert(taxable_amt + "==" + d_amt +"=="+ d_0_p + " == Dipesh Tax")
        if (taxable_amt <= d_amt) {
            //alert("if")
            t_0_a = taxable_amt * d_0_p / 100;
        } else {
            //alert("else")
            t_0_a = d_amt * d_0_p / 100;
            taxable_new_amt = taxable_amt - d_amt;
            if (taxable_new_amt <= d_1_a) {
                t_1_a = taxable_new_amt * d_1_p / 100;
            } else {
                t_1_a = d_1_a * d_1_p / 100;
                taxable_new_amt -= d_1_a;
                if (taxable_new_amt <= d_2_a) {
                    t_2_a = taxable_new_amt * d_2_p / 100;
                } else {
                    t_2_a = d_2_a * d_2_p / 100;
                    taxable_new_amt -= d_2_a;
                    if (taxable_new_amt <= d_3_a) {
                        t_3_a = taxable_new_amt * d_3_p / 100;
                    } else {
                        t_3_a = d_3_a * d_3_p / 100;
                        taxable_new_amt -= d_3_a;
                        if (taxable_new_amt <= fourth_tax_amount) {
                            t_4_a = taxable_new_amt * d_4_p / 100;
                        } else {
                            t_4_a = fourth_tax_amount * d_4_p / 100;
                            taxable_new_amt -= fourth_tax_amount;
                            t_5_a = taxable_new_amt * fifth_tax_percent / 100;
                        }
                    }
                }
            }
        }

    t_a_x = t_0_a + t_1_a + t_2_a + t_3_a + t_4_a + t_5_a;
    // alert(d_amt + '--' + d_amt_s + '--' + t_0_a + "==" + t_1_a + "==" + t_2_a + "==" + t_3_a + "==" + t_4_a + "==" + t_5_a + " ==Dipesh Taxamble")
        // Female deduction
        if (d_amt == d_amt_s) {
            t_0_a -= (t_0_a * gender_ded / 100);
            t_a_x -= (t_a_x * gender_ded / 100);
        }

        /*
        YEARLY TAX DISPLAY
        */
        setValue("yearly_gross_tax", t_a_x.toFixed(0));
        setValue("yearly_gross_tax_h", t_a_x.toFixed(0));

        // SUBTRACT ALREADY PAID TAX AMOUNT
        t_a_x = t_a_x - (tax_dud + pre_access_tax_taken + pre_access_tax);
        t_a_x = t_a_x - (medical_deduction_on_tax_taken + medical_deduction_on_tax);
        t_a_x = parseFloat(t_a_x.toFixed(0));

        // MONTHLY TAX CALCULATION
        t_a_x = t_a_x / month_diff;
        if (t_a_x < 0) { t_a_x = 0; }

        /*
        CALCULATE NET IN HAND
        */
        net_in_hand += basic_salary + pf_a + gratuity + ssf;
        net_in_hand += performance_all + insurance + others + children_edu_all;
        net_in_hand += remote_area_all + overtime + medical_expense_reimburse_total + leave_encash;
        if (isChecked("is_dashain")) { net_in_hand += dashain_a; }
        net_in_hand -= (pf_d + cit_d + betalibi_d + gratuity_ded + ssf_ded);

        /*
        MONTH TOTAL
        */
        setValue("monthly_gross_salary", net_in_hand.toFixed(0));
        setValue("monthly_gross_salary_h", net_in_hand.toFixed(0));

        /*
        MONTH TAX TOTAL
        */
        setValue("incometax_d", t_a_x.toFixed(0));
        setValue("incometax_d_h", t_a_x.toFixed(0));

        /*
        MONTH GROSS_SALARY_AFTER_TAX
        */
        net_in_hand = net_in_hand - t_a_x;
        setValue("gross_salary_after_tax", net_in_hand.toFixed(0));
        setValue("gross_salary_after_tax_h", net_in_hand.toFixed(0));

        // calling net take home calculator
        calculate_net_amount(ival);
    }
    function cal_dashain_with_tax(is_dashain_already, basic_salary, ival) {
        var dashain_amount = 0;

        if (is_dashain_already !== "Y") {
            var d_0_p = getParseFloatValue("d_0_p", "");
            var d_1_p = getParseFloatValue("d_1_p", "");
            var d_2_p = getParseFloatValue("d_2_p", "");
            var d_3_p = getParseFloatValue("d_3_p", "");
            var d_4_p = getParseFloatValue("d_4_p", "");
            var d_amt_s = getParseFloatValue("d_amt_s", ""); // marital deduction for single
            var count_days_dashain = getParseFloatValue("count_days_dashain", ival);
            var gender_ded = getParseFloatValue("gender_ded", ival); // female deduction
            var d_amt = getParseFloatValue("d_amt", ival); // marital deduction for employee
            var fifth_tax_percent = getParseFloatValue("fifth_tax_percent", "");

            /*
            SUBTRACT % FOR FEMALE | Single : Yes | Married : No
            */
            var d_0_p_f = 0, d_1_p_f = 0, d_2_p_f = 0, d_3_p_f = 0, d_4_p_f = 0, d_5_p_f = 0;
            if (d_amt === d_amt_s && gender_ded !== 0) {
                d_0_p_f = parseFloat(d_0_p * gender_ded / 100);
                d_1_p_f = parseFloat(d_1_p * gender_ded / 100);
                d_2_p_f = parseFloat(d_2_p * gender_ded / 100);
                d_3_p_f = parseFloat(d_3_p * gender_ded / 100);
                d_4_p_f = parseFloat(d_4_p * gender_ded / 100);
                d_5_p_f = parseFloat(fifth_tax_percent * gender_ded / 100);
            }

            /* Get Divider */
            var d_0_p_1 = 100 - (d_0_p - d_0_p_f);
            var d_1_p_2 = 100 - (d_1_p - d_1_p_f);
            var d_2_p_3 = 100 - (d_2_p - d_2_p_f);
            var d_3_p_4 = 100 - (d_3_p - d_3_p_f);
            var d_4_p_5 = 100 - (d_4_p - d_4_p_f);
            var d_5_p_6 = 100 - (fifth_tax_percent - d_5_p_f);

            var percent_for_tax_add = rdo_selected_percent(ival);

            if (percent_for_tax_add === "r0") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_0_p_1;
            } else if (percent_for_tax_add === "r1") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_1_p_2;
            } else if (percent_for_tax_add === "r2") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_2_p_3;
            } else if (percent_for_tax_add === "r3") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_3_p_4;
            } else if (percent_for_tax_add === "r4") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_4_p_5;
            } else if (percent_for_tax_add === "r5") {
                dashain_amount = parseFloat(basic_salary) * 100 / d_5_p_6;
            }

            /*
            count_days_dashain:
            = -2 => Special case for No Dashain Amount
            = -1 => No dashain (join date > Sep 30)
            = 0  => Full Dashain Amount
            > 0  => Pro-rata Dashain Amount
            */
            if (count_days_dashain === 0) {
                // full amount
            } else if (count_days_dashain > 0) {
                dashain_amount = dashain_amount / 365;
                dashain_amount = dashain_amount * count_days_dashain;
            } else if (count_days_dashain === -1 || count_days_dashain === -2) {
                dashain_amount = 0;
            }
        }

        return dashain_amount;
    }
    function calculate_net_amount(ival) {
        // NET TAKE HOME CALCULATOR
        var net_in_hand_real = 0;
        var welfare_fund = 0;

        // Inputs
        var basic_salary = getParseFloatValue("basic_salary", ival);
        var welfare_fund_per = getParseFloatValue("welfare_fund_per", ival);
        var net_monthly_gross_salary = getParseFloatValue("monthly_gross_salary", ival); // NET MONTH SALARY
        var net_incometax_d = getParseFloatValue("incometax_d", ival); // TAX

        var pe_adv = getParseFloatValue("txtadvpe", ival); // Personnel advance
        var pr_adv = getParseFloatValue("txtadvpr", ival); // Program advance
        var tr_adv = getParseFloatValue("txtadvtr", ival); // Travel advance
        var fd_adv = getParseFloatValue("txtadvfd", ival); // Field drawing advance
        var adv_pf_loan = getParseFloatValue("txtadvpf", ival); // PF loan
        var adv_cit_loan = getParseFloatValue("txtadvcit", ival); // CIT loan
        var wl_adv = getParseFloatValue("txtadvwl", ival); // Welfare loan

        // WELFARE FUND OF THIS MONTH
        basic_salary = parseFloat(basic_salary);
        welfare_fund_per = parseFloat(welfare_fund_per);
        welfare_fund = (basic_salary * welfare_fund_per) / 100;
        welfare_fund = parseFloat(welfare_fund);

        var wfEls = document.getElementsByName("welfare_fund" + ival);
        if (wfEls.length > 0) wfEls[0].value = welfare_fund.toFixed(0);

        var wfElsH = document.getElementsByName("welfare_fund_h" + ival);
        if (wfElsH.length > 0) wfElsH[0].value = welfare_fund.toFixed(0);

        // MONTH GROSS_SALARY_AFTER_TAX
        net_in_hand_real = net_monthly_gross_salary - net_incometax_d;
        net_in_hand_real = parseFloat(net_in_hand_real);

        var gsEls = document.getElementsByName("gross_salary_after_tax" + ival);
        if (gsEls.length > 0) gsEls[0].value = net_in_hand_real.toFixed(0);

        var gsElsH = document.getElementsByName("gross_salary_after_tax_h" + ival);
        if (gsElsH.length > 0) gsElsH[0].value = net_in_hand_real.toFixed(0);

        // Subtract advances and welfare fund
        net_in_hand_real = net_in_hand_real - (
            pe_adv + pr_adv + tr_adv + fd_adv + wl_adv + adv_pf_loan + adv_cit_loan + welfare_fund
        );

        var niEls = document.getElementsByName("net_in_hand" + ival);
        if (niEls.length > 0) niEls[0].value = net_in_hand_real.toFixed(0);

        var niElsH = document.getElementsByName("net_in_hand_h" + ival);
        if (niElsH.length > 0) niElsH[0].value = net_in_hand_real.toFixed(0);
    }

function postdata_auto_calculate_salary_bulk() {
    // Get record count safely
    var recCountEls = document.getElementsByName("HRecCount");
    var jj = 0;
    if (recCountEls.length > 0) {
        jj = parseInt(recCountEls[0].value) || 0;
    }

    // Loop through records
    for (var intCount = 1; intCount <= jj; intCount++) {
        // Get the first matching checkbox and textbox
        var checkboxEls = document.getElementsByName("is_dashain" + intCount);
        var textboxEls = document.getElementsByName("is_dashain_check" + intCount);
        //alert(checkboxEls + '--' + textboxEls);
        if (checkboxEls.length > 0 && textboxEls.length > 0) {
            var checkbox = checkboxEls[0];
            var textbox = textboxEls[0];

            // Debugging
            console.log("Row " + intCount + " checkbox checked:", checkbox.checked);

            // If checked, assign its value; otherwise set "N"
            if (checkbox.checked) {
                textbox.value = checkbox.value;   // usually "Y" or "on"
            } else {
                textbox.value = "N";
            }
            calculate_employee_salary_new(intCount);
        }
    }
}

    function checkInput(type, cname, msg) {
        let flag = true;
        let ctrl = document.getElementById(cname);
            
        switch (type) {

            case 'cmb':
                if (!ctrl || !ctrl.value || ctrl.value === 'session_off') {
                    flag = false;
                }
                break;
            default:
                console.warn(`Unknown input type: ${type}`);
                flag = false;
        }

        return flag ? "" : "\n" + msg;
    }

