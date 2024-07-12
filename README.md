Form Filler Core
====================
This program was design to allow filling of PDF Forms through the use of API using iText Sharp.  It allows a user to supply a JSON object to an API end point and then either email the PDF to the supplied email address or return the filled PDF in a byte array for the
user to use.  It also contains the ability to create small pdfs in a bolded question/unbolded answer format and to swap out data in a html string based off a data map.  The filler also has support for repeating fields and calculated fields.

End Points
---------------------
+ #### [url]/api/FormApi/GetDataSchema?form=[formname]&datatype=[datatype]
    This end point will return the data map schema for the specified form in a JSON object

+ #### [url]/api/FormApi/FillForm?form=[formname]&datatype=[datatype]
    This end point will take the JSON object **JOBject** provided in the request body and then replace all the form fields in the datamap with the value supplied.  It will then send the PDF back as a byte array to be used.
  
+ #### [url]/api/FormApi/EmailForm?form=[formname]&datatype=[datatype]
    Takes a modified version of the **JOBject** object provided in the request body that contains the email address to fill the form fields.  It will then email the file to the provided email addresses.

+ #### [url]/api/FormApi/CreateForm?form=[formname]&datatype=[datatype]
    Take the **JObject** object and then build a pdf from it.  The corresponding pdf is in the format of Large bolded title in the top and all fields in the datamap created bolded with the values unbolded beneath them.

+ #### [url]/api/FormApi/CreateEmailForm?form=[formname]&datatype=[datatype]
    This end point will do all that the CreateForm endpoint will do and it will email the resulting file to the specified email addresses.



TO DO
---------------------
  + Add **JObject** specifications to readme file
  + Add overview of reapeating field, calculated fileds, and html string replacement to readme
  + Modify app to use Graph to send email (Microsoft has cut off support for SMTP)
