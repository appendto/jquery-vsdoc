jQuery.mobile = function(){
		/// <summary>
		/// 	The root jQuery Mobile Object.
		/// </summary>
};

$.extend(jQuery.mobile, {
	changePage: function( to, transition, back, changeHash ){
		/// <summary>
		/// 	Programmatically change from one page to another. This method is used internally for transitions that occur as a result of clicking a link or submitting a form, when those features are enabled.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. $.mobile.addResolutionBreakpoints( arrayOfNumbers ) - Pass an array of numbers to add min/max classes for multiple widths.
		/// </summary>
		///	<param name="to" type="String">
		///		&#10;&#09;1. String - A url to transition to.
		///		&#10;2. jQuery - A jQuery object to transition to.
		///		&#10;3. Array - Array specifying two page references [from,to] for transitioning from a known page. From is otherwise assumed to be the current page in view (or $.mobile.activePage ).
		///		&#10;4. Object - Object for sending form data. ({to: url, data: serialized form data, type: 'get' or 'post'}
		/// </param>
		///	<param name="transition" type="String" optional="true">
		/// 	Specifies the effect to use when switching between pages. eg. 'pop', 'slide', 'none'.
		/// </param>
		///	<param name="back" type="Boolean" optional="true">
		/// 	Default value is false. True will cause a reverse-direction transition.
		/// </param>
		///	<param name="changeHash" type="Boolean" optional="true">
		/// 	Default value is true. Update the hash to the to page's URL when page change is complete.
		/// </param>
	},
	pageLoading: function( done ){
		/// <summary>
		/// 	Show or hide the page loading message, which is configurable via $.mobile.loadingMessage.
		/// </summary>
		///	<param name="done" type="Boolean">
		/// 	Defaults value is false, indicating loading has started. A value of true will hide the loading message.
		/// </param>
	},
	silentScroll: function( yPos ){
		/// <summary>
		/// 	Scroll to a particular Y position without triggering scroll event listeners.
		/// </summary>
		///	<param name="yPos" type="Number" integer="true">
		/// 	Specifies a y-axis position that the page should scroll to.
		/// </param>
	},
	addResolutionBreakpoints: function( width ){
		/// <summary>
		/// 	Add width breakpoints to the min/max width classes that are added to the HTML element.
		/// 	&#10;Additional Signatures:
		/// 	&#10;&#09;1. $.mobile.addResolutionBreakpoints( arrayOfNumbers ) - Pass an array of numbers to add min/max classes for multiple widths.
		/// </summary>
		///	<param name="width" type="Number" integer="true">
		/// 	A number or array of numbers to add to the resolution classes.
		/// </param>
	},
	activePage: function(){
		/// <summary>
		/// 	Returns a reference to the page currently in view.
		/// </summary>
		///	<param name="property" type="Object">
		/// 	Unknown.
		/// </param>		
		/// <returns type="Object" />
	},
	media: function( query ){
		/// <summary>
		/// 	A function that allows you to test whether a particular CSS Media Query applies.
		/// </summary>
		///	<param name="selector" type="String">
		/// 	A string specifyin the query or css type to be queried.
		/// 	&#10;&#10;//test for screen media type
		/// 	$.mobile.media("screen");
		/// 	&#10;&#10;//test  a min-width media query
		/// 	$.mobile.media("screen and (min-width: 480px)");
		/// 	&#10;&#10;//test for iOS retina display
		/// 	$.mobile.media("screen and (-webkit-min-device-pixel-ratio: 2)");
		/// </param>
		/// <returns type="Boolean">
		/// 	If the browser supports the type or query specified, and it currently applies, the function will return true. If not, it'll return false.
		/// </returns>	
	}
});

		/// <summary>
		/// 	
		/// </summary>
		///	<param name="selector" type="String">
		/// 	
		/// </param>
		/// <returns type="Boolean">
		/// 	
		/// </returns>