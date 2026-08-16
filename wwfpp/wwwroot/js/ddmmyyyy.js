var my_format = "DD/MM/YYYY";

function convert_date(field1)
{
var fLength = field1.value.length; // Length of supplied field in characters.
var divider_values = new Array ('-','.','/',' ',':','_',','); // Array to hold permitted date seperators.  Add in '\' value
var array_elements = 7; // Number of elements in the array - divider_values.
var day1 = new String(null); // day value holder
var month1 = new String(null); // month value holder
var year1 = new String(null); // year value holder
var divider1 = null; // divider holder
var outdate1 = null; // formatted date to send back to calling field holder
var counter1 = 0; // counter for divider looping 
var divider_holder = new Array ('0','0','0'); // array to hold positions of dividers in dates
var s = String(field1.value); // supplied date value variable

//If field is empty do nothing
if ( fLength == 0 ) {
   return true;
}
// Deal with today or now
if ( field1.value.toUpperCase() == 'NOW' || field1.value.toUpperCase() == 'TODAY' ) {
   
 var newDate1 = new Date();
 
      if (navigator.appName == "Netscape") {
        var myYear1 = newDate1.getYear() + 1900;
      }
      else {
        var myYear1 =newDate1.getYear();
      }
  
 var myMonth1 = newDate1.getMonth()+1;  
 var myDay1 = newDate1.getDate();
 field1.value = myDay1 + "/" + myMonth1 + "/" + myYear1;
 fLength = field1.value.length;//re-evaluate string length.
 s = String(field1.value)//re-evaluate the string value.
}

//Check the date is the required length
if ( fLength != 0 && (fLength < 6 || fLength > 11) ) {
 invalid_date(field1);
 return false;   
 }

// Find position and type of divider in the date
for ( var i=0; i<3; i++ ) {
 for ( var x=0; x<array_elements; x++ ) {
  if ( s.indexOf(divider_values[x], counter1) != -1 ) {
    divider1 = divider_values[x];
    divider_holder[i] = s.indexOf(divider_values[x], counter1);
     //alert(i + " divider1 = " + divider_holder[i]);
    counter1 = divider_holder[i] + 1;
    //alert(i + " counter1 = " + counter1);
    break;
  }
  }
 }

// if element 2 is not 0 then more than 2 dividers have been found so date is invalid.
if ( divider_holder[2] != 0 ) {
   invalid_date(field1);
 return false;   
}

// See if no dividers are present in the date string.
if ( divider_holder[0] == 0 && divider_holder[1] == 0 ) { 
   
  //continue processing
  if ( fLength == 6 ) {//ddmmyy
      day1 = field1.value.substring(0,2);
        month1 = field1.value.substring(2,4);
        year1 = field1.value.substring(4,6);
        if ( (year1 = validate_year(year1)) == false ) {
        invalid_date(field1);
      return false; 
      }
    }
    
  else if ( fLength == 7 ) {//ddmmmy
      day1 = field1.value.substring(0,2);
        month1 = field1.value.substring(2,5);
        year1 = field1.value.substring(5,7);
        if ( (month1 = convert_month(month1)) == false ) {
        invalid_date(field1);
      return false; 
      }
        if ( (year1 = validate_year(year1)) == false ) {
        invalid_date(field1);
      return false; 
      }
    }
  else if ( fLength == 8 ) {//ddmmyyyy
      day1 = field1.value.substring(0,2);
        month1 = field1.value.substring(2,4);
        year1 = field1.value.substring(4,8);
    }
  else if ( fLength == 9 ) {//ddmmmyyyy
      day1 = field1.value.substring(0,2);
        month1 = field1.value.substring(2,5);
        year1 = field1.value.substring(5,9);
        if ( (month1 = convert_month(month1)) == false ) {
        invalid_date(field1);
      return false; 
      }
    }
  
  if ( (outdate1 = validate_date(day1,month1,year1)) == false ) {
    alert(msg_the_value + field1.value + msg_is_not_valid_date + msg_please_enter_valid_date_format);
    field1.focus();
    field1.select();
    return false;
    }

  field1.value = outdate1;
  return true;// All OK
  }
  
// 2 dividers are present so continue to process  
if ( divider_holder[0] != 0 && divider_holder[1] != 0 ) {   
    day1 = field1.value.substring(0, divider_holder[0]);
    month1 = field1.value.substring(divider_holder[0] + 1, divider_holder[1]);
    //alert(month1);
    year1 = field1.value.substring(divider_holder[1] + 1, field1.value.length);
 }

if ( isNaN(day1) && isNaN(year1) ) { // Check day and year are numeric
 invalid_date(field1);
 return false;  
   }

if ( day1.length == 1 ) { //Make d day dd
   day1 = '0' + day1;  
}

if ( month1.length == 1 ) {//Make m month mm
 month1 = '0' + month1;   
}

if ( year1.length == 2 ) {//Make yy year yyyy
   if ( (year1 = validate_year(year1)) == false ) {
    invalid_date(field1);
  return false;  
  }
}

if ( month1.length == 3 || month1.length == 4 ) {//Make mmm month mm
   if ( (month1 = convert_month(month1)) == false) {
    alert("month1" + month1);
    invalid_date(field1);
    return false;  
   }
}

// Date components are OK
if ( (day1.length == 2 || month1.length == 2 || year1.length == 4) == false) {
   invalid_date(field1);
   return false;
}

//Validate the date
if ( (outdate1 = validate_date(day1, month1, year1)) == false ) {
 alert(msg_the_value + field1.value + msg_is_not_valid_date + msg_please_enter_valid_date_format);
 field1.focus();
 field1.select();
 return false;
}

// Redisplay the date in dd/mm/yyyy format
field1.value = outdate1;
return true;//All is well

}

function convert_month(monthIn) {

var month_values = new Array ("JAN","FEB","MAR","APR","MAY","JUN","JUL","AUG","SEP","OCT","NOV","DEC");

monthIn = monthIn.toUpperCase(); 

if ( monthIn.length == 3 ) {
 for ( var i=0; i<12; i++ ) 
  {
    if ( monthIn == month_values[i] ) 
      {
    monthIn = i + 1;
    if ( i != 10 && i != 11 && i != 12 ) 
      {
        monthIn = '0' + monthIn;
      }
    return monthIn;
    }
  }
 }

else if ( monthIn.length == 4 && monthIn == 'SEPT') {
   monthIn = '09';
   return monthIn;
 }
 
else {
 return false;
 } 
}
function invalid_date(inField) 
{
alert(msg_the_value + inField.value + msg_is_not_valid_date_format + msg_please_enter_date_in_the_format);
inField.focus();
inField.select();
inField.value = "";
return true;   
}

function validate_date(day2, month2, year2)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
{                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
var DayArray = new Array(31,28,31,30,31,30,31,31,30,31,30,31);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
var MonthArray = new Array("01","02","03","04","05","06","07","08","09","10","11","12");                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
var inpDate = day2 + month2 + year2;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
var filter=/^[0-9]{2}[0-9]{2}[0-9]{4}$/;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          

//Check ddmmyyyy date supplied
if (! filter.test(inpDate))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
  {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
  return false;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
  }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
/* Check Valid Month */                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
filter=/01|02|03|04|05|06|07|08|09|10|11|12/ ;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
if (! filter.test(month2))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
  {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
  return false;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
  }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
/* Check For Leap Year */                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
var N = Number(year2);
if ( ( N%4==0 && N%100 !=0 ) || ( N%400==0 ) )                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
    {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
   DayArray[1]=29;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
    }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
/* Check for valid days for month */                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
for(var ctr=0; ctr<=11; ctr++)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
    {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
   if (MonthArray[ctr]==month2)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
    {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
      if (day2<= DayArray[ctr] && day2 >0 )                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
        {
        inpDate = day2 + '/' + month2 + '/' + year2;       
        return inpDate;
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
      else                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
        return false;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
    }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
   }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
}

function validate_year(inYear) 
{
if ( inYear < 30 ) 
 {
   inYear = "20" + inYear;
   return inYear;
 }
else if ( inYear >= 30 )
 {
   inYear = "19" + inYear;
   return inYear;
 }
else 
 {
 return false;
 }   
}
