/// <reference path="/js/test.js" />
/// <reference path="/js/jquery-1.5-vsdoc.js" />


$('div').delegate(



























// http://weblogs.asp.net/bleroy/archive/2007/04/23/the-format-for-javascript-doc-comments.aspx
// http://www.scottlogic.co.uk/2010/08/vs-2010-vs-doc-and-javascript-intellisense/
// http://skysanders.net/subtext/archive/2010/02/22/visual-studio-javascript-intellisense-uncovered.aspx
// http://skysanders.net/subtext/archive/2010/02/23/jquery-ajax-settings-object-for-vs-intellisense.aspx

//<summary locid="descriptionID">Description</summary>

//<param name="parameterName"
//	mayBeNull="true|false" 
//	optional="true|false"
//  type="ParameterType" 
//	parameterArray="true|false"
//  integer="true|false" 
//	domElement="true|false"
//  elementType="ArrayElementType" 
//	elementInteger="true|false"
//  elementDomElement="true|false"
//  elementMayBeNull="true|false"
//>Description</param>

//<returns
//  type="ValueType" 
//	integer="true|false" 
//	domElement="true|false"
//  mayBeNull="true|false" 
//	elementType="ArrayElementType"
//  elementInteger="true|false" 
//	elementDomElement="true|false"
//  elementMayBeNull="true|false"
//>Description</returns>

//<value
//  type="ValueType" 
//	integer="true|false" 
//	domElement="true|false"
//  mayBeNull="true|false" 
//	elementType="ArrayElementType"
//  elementInteger="true|false" 
//	elementDomElement="true|false"
//  elementMayBeNull="true|false"
//  locid="descriptionID"
//>Description</value>

//The value tag describes a property (which shouldn't have a summary tag).

//<field name="fieldName" 
//	type="FieldType"
//	integer="true|false" 
//	domElement="true|false" 
//	mayBeNull="true|false"
//	elementType="ArrayElementType" 
//	elementInteger="true|false"
//	elementDomElement="true|false" 
//	elementMayBeNull="true|false"
//	locid="descriptionID"
//>Description</field>

//The field tag (which doesn't exist in C# and VB.NET because in those languages the 
//summary tag can be used) is used inside of a class, interface or enumeration 
//constructor to describe a field. A field can't be described near the field 
//itself in JavaScript because the field itself doesn't have a body to contain 
//that description.