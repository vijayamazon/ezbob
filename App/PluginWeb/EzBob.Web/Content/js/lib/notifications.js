
/*
* Shiny Notification Boxes - for jQuery 1.3+
* http://codecanyon.net/item/shiny-notification-boxes/150512
*
* Copyright 2011, Rik de Vos
* You need to buy a license if you want use this script.
* http://codecanyon.net/wiki/buying/howto-buying/licensing/
*
* Version: 1.1.1 (Feb 6 2011)
*/

(function ($) {
    $.fn.notification = function(closeText) {

        //Define variables

        var colors = ["green", "yellow", "orange", "red", "pink", "darkblue", "lightblue", "white"], //Set the colors
            close = "",
            specular = "",
            notifications = "",
            o = false;

        setVariables();
        prepNotifications();
        effect_blink();
        effect_slim();
        effect_fadein();
        closeButton();
        closeNotification();
        fixBrowsers();


        function setVariables() {
            for (var t = 0; t < colors.length; t++) { //Loop through the colors and set the variables

                //Close class
                (t !== 7) ? close = close + ".notification_" + colors[t] + " .close, " : close = close + ".notification_" + colors[t] + " .close";

                //Specular class
                (t !== 7) ? specular = specular + ".notification_" + colors[t] + " .specular, " : specular = specular + ".notification_" + colors[t] + " .specular";

                //Notification class
                (t !== 7) ? notifications = notifications + ".notification_" + colors[t] + ", " : notifications = notifications + ".notification_" + colors[t];
            }
        }

        function prepNotifications() {

            //Go through all the notifications and prepare them

            $(notifications).each(function() {

                var $this = $(this);

                $this.html('<div class="innerText" style="display: inline; vertical-align: middle;">' + $this.html() + '</div>'); //Wrap the text in a new element

                if (typeof window.ActiveXObject == "undefined") {
                    $this.append('<div class="specular_wrapper"><div class="specular"></div></div>'); //Add a specular element
                }


                if (!$this.hasClass("effect_fixed")) {
                	$this.append('<div class="close">' + closeText +'</div>'); //Append a close button if the notification hasn't got a fixed class
                }


                if ($this.attr("href")) {


                    $this.click(function() {
                        if (!o) {
                            window.location = $this.attr("href"); //Set the new location
                        }
                    });


                    $this.addClass("effect_href"); //Add a href class to make life easier
                }

                if (typeof window.ActiveXObject !== "undefined") {
                    //If Internet Explorer

                    if ($this.hasClass("effect_blink")) {
                        $this.attr("filter", $this.css("filter")); //Save the background gradient in an attribute
                    }

                    if (parseFloat(navigator.appVersion.split("MSIE")[1]) < 7) {
                        //IE6
                        $this.hide(0).show(0); //Seems to fix the close button bug
                        colorToBackground($this.css("color"), $this); //Set the gradient background
                    }
                }

            });
        }

        function effect_blink() {

            //Blink class

            $(".effect_blink").hover(function() {
                $(this).removeClass("effect_blink").css("opacity", 1); //Remove the blink class when the user hovers over the notification
            });


            var x = 0;

            setInterval(function() {

                //Update the new opacity

                if (typeof window.ActiveXObject == "undefined") {
                    //If not IE
                    $(".effect_blink").fadeTo(0, Math.abs(Math.sin(x)) * 0.2 + 0.8); //Calculate the new opacity
                } else {
                    //Runs when the browser is IE
                    $(".effect_blink").each(function() {
                        $(this).css("filter", $(this).attr("filter") + "progid:DXImageTransform.Microsoft.Alpha(Opacity=" + (Math.abs(Math.sin(x)) * 0.4 + 0.6) * 100 + ")"); //Set a new filter: the new opacity and the gradient
                    });

                }

                x += 0.06; //Increase the x

            }, 20);
        }

        function effect_slim() {

            //Slim class

            $(".effect_slim").each(function() {
                var innerWidth = $(this).children(".innerText").innerWidth(); //Set the innerwidth of the object to a minimum
                $(this).css("width", innerWidth + "px");
            });
        }

        function effect_fadein() {

            //Fade in class

            $(".effect_fadein").css("opacity", 0).animate({
                //Animate the opacity
                    "opacity": 1
                }, 2000, function() {
                    if (typeof window.ActiveXObject !== "undefined") {
                        colorToBackground($(this).css("color"), $(this)); //If IE, set the background gradient
                    }
                });
        }

        function colorToBackground(color, $this) {

            //Function for applying a background gradient if the text color is given

            var begin = "progid:DXImageTransform.Microsoft.gradient(startColorstr="; //Save this for regular use below

            switch (color) {
            case "#47642f":
                $this.css("filter", begin + "#b0e27b, endColorstr=#6ea54b)"); //Green
                break;
            case "#62642f":
                $this.css("filter", begin + "#d3e37c, endColorstr=#9fa248)"); //Yellow
                break;
            case "#64472f":
                $this.css("filter", begin + "#e2b07b, endColorstr=#a4714a)"); //Orange
                break;
            case "#642f2f":
                $this.css("filter", begin + "#e27b7b, endColorstr=#a44a4a)"); //Red
                break;
            case "#642f62":
                $this.css("filter", begin + "#e27bcb, endColorstr=#a44a98)"); //Pink
                break;
            case "#342f64":
                $this.css("filter", begin + "#7f7be2, endColorstr=#4a4ca4)"); //Dark blue
                break;
            case "#2f4d64":
                $this.css("filter", begin + "#7bb0e1, endColorstr=#4a7da4)"); //Light blue
                break;
            case "#585858":
                $this.css("filter", begin + "#eaeaea, endColorstr=#909191)"); //White
                break;
            default:
            //No default case needed
            }
        }

        function closeButton() {

            //When the notification is a link, hovering over the close button must deactivate the link

            $(close).hover(function() {
                o = true;
            }, function() {
                o = false;
            });
        }

        function closeNotification() {

            //Function to close the notification

            $(close).click(function() {
                $(this)
                    .parent(notifications)
                    .animate({
                            "opacity": 0,
                            "min-height": 0
                        }, function() {
                            $(this).hide(500);
                        });
                $(this).fadeOut();
            });
        }

        function fixBrowsers() {

            //Fix the opera issues

            if (typeof window.opera !== "undefined") {
                $(specular).css({
                //Re-size the specular element
                    "top": "-50%",
                    "left": "-5%",
                    "height": "130%"
                });
                $(notifications).css("border-radius", "5px");
                $(close).css({
                    "border-radius": "7px"
                });
            }

            //Fix IE issues

            if (typeof window.ActiveXObject !== "undefined") {
                var bgColors = ["#7c9a5e", "#939c60", "#9b7d5f", "#9a5e5e", "#9a5e8e", "#605e9a", "#5f7e9b", "#9c9c9c"];
                var closes = close.split(", ");
                for (var i = 0; i < closes.length; i++) {
                    $(closes[i]).css({ "background": bgColors[i] });
                }
                $(close).css({
                //Reposition the close button
                    "top": "4px",
                    "right": "4px"
                });
                $(".text").css({ "left": "10px" }); //Reposition the text

            }
        }
    };
}
)(jQuery);