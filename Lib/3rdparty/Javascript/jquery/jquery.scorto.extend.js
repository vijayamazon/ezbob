//--------------------------------------------------------------------------------------
// Scorto jQuery.validator new validation methods
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoTextStringRequired", 
    function(value, element)
    {
        return RequiredStringValidation(value, element, '');
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoTextNumericRequired", 
    function(value, element)
    {
        return RequiredStringValidation(value, element, '0');
    }, ' '
);
//--------------------------------------------------------------------------------------
    jQuery.validator.addMethod("ScortoGridRequired",
    function(value, element) {
        var jElement = $(element);
        var UpdatePanel = jElement.parent('.DPGrid');
        var button = UpdatePanel.find('.DPGridHeader input');
        if (button.attr('disabled') || jQuery.validator.ScortoIsOutlet == false)
            return Scorto_ClearError(UpdatePanel, 'ScortoElementRequireError');

        var number = Number(jElement.attr('rowsCount'));

        if (number != 0) {
            return Scorto_ClearError(UpdatePanel, 'ScortoElementRequireError');
        }
        else {
            return Scorto_SetError(UpdatePanel, 'ScortoElementRequireError');
        }
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoTextRegex", 
    function(value, element)
    {
        var jElement = $(element);
        if (element.disabled == true)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        var strToValidate = value.replace(/\n/g, '');
        if (strToValidate == '') return Scorto_AddValidationToolTip(jElement, '');
        var regex = new RegExp(jElement.attr('RegexString'));

        if(!regex.test(strToValidate))
        {
            return Scorto_AddValidationToolTip(jElement, jElement.attr('ErrorMessage'));
        }
        
        return Scorto_AddValidationToolTip(jElement, '');
     }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoElementRequired", 
    function(value, element)
    {
        var jElement = $(element);
        if (jQuery.validator.ScortoIsOutlet == false)
        {
            return Scorto_ClearError(jElement, 'ScortoElementRequireError');
        }
        var emptyItem = jElement.attr('comboboxDefaultValue');
        if(emptyItem != null)
        {
            var comboboxDecorator = $('#' + element.id + '_NewCombobox');
            if(comboboxDecorator.length != 0)
                jElement = comboboxDecorator;
                
            if(element.disabled == true || element.value != emptyItem)
            {
                return Scorto_ClearError(jElement, 'ScortoElementRequireError');
            }
            else 
            {
                return Scorto_SetError(jElement, 'ScortoElementRequireError');
            }
        }
        return true;
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoAttachmentMinMax", 
    function(value, element)
    {
        return Scorto_AttachValidate(value, element);
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoNumericValidator", 
    function(value, element)
    {
        var jElement = $(element);
        if (element.disabled == true)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        
        if(isNaN(GetValidNumber(value)))
        {
            return Scorto_AddValidationToolTip(jElement, jElement.attr('ErrorMessage'));
        }
        else
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoRangeValidator", 
    function(value, element)
    {
        var jElement = $(element);
        if (element.disabled == true)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        
        var val = GetValidNumber(value);
        var min = GetValidNumber(jElement.attr('minValue'));
        var max = GetValidNumber(jElement.attr('maxValue'));
        
        if(isNaN(val) 
        || isNaN(val) 
        || isNaN(val))
        {
            return Scorto_AddValidationToolTip(jElement, jElement.attr('ErrorMessage'));        
        }
        
        if(val < min || val > max)
        {
            return Scorto_AddValidationToolTip(jElement, jElement.attr('ErrorMessage'));
        }
        
        return Scorto_AddValidationToolTip(jElement, '');
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoDateValidator", 
    function(value, element)
    {
        var jElement = $(element);
    
        var calendarObj = element.ScortoCalendarBehavior;
        if(calendarObj == null)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        if (element.disabled)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        
        var args = { IsValid : false };
        calendarObj._validateTextValue(element, args);
        
        if(args.IsValid)
        {
            return Scorto_AddValidationToolTip(jElement, '');
        }
        return Scorto_AddValidationToolTip(jElement, jElement.attr('ErrorMessage'));  
    }, ' '
);
//--------------------------------------------------------------------------------------
jQuery.validator.addMethod("ScortoRequireDateValidator", 
    function(value, element)
    {
        var jElement = $(element);
        if (jQuery.validator.ScortoIsOutlet == false)
        {
            return Scorto_ClearError(jElement, 'ScortoElementRequireError');
        }
    
        var calendarObj = element.ScortoCalendarBehavior;
        if(calendarObj == null)
        {
            return Scorto_ClearError(jElement, 'ScortoElementRequireError');
        }
        
        if (calendarObj._maskBehaviorId && element.disabled == false)
        {
            var maskEdit = $find(calendarObj._maskBehaviorId);
            if (maskEdit)
            {
                if (maskEdit._EmptyMask == value)
                {
                    Scorto_AddValidationToolTip(jElement, '');
                    return Scorto_SetError(jElement, 'ScortoElementRequireError');   
                }
            }
        }
        return Scorto_ClearError(jElement, 'ScortoElementRequireError');
    }, ' '
);

//--------------------------------------------------------------------------------------
// Scorto helper validation functions
//--------------------------------------------------------------------------------------
function RequiredStringValidation(value, element, defaultString)
{
        var jElement = $(element);
        if (jQuery.validator.ScortoIsOutlet == false)
        {
            return Scorto_ClearError(jElement, 'ScortoElementRequireError');
        }
        var maskObj = element.MaskedEditBehavior;
        if(maskObj != null)
        {
            value = maskObj._getClearMask();
        }

        if(value != defaultString || element.disabled == true)
        {
             return Scorto_ClearError(jElement, 'ScortoElementRequireError');
        }
        return Scorto_SetError(jElement, 'ScortoElementRequireError');
}
//--------------------------------------------------------------------------------------
function Scorto_SetError(jElement, cssClass)
{
    jElement.addClass(cssClass);
    return false;
}
//--------------------------------------------------------------------------------------
function Scorto_ClearError(jElement, cssClass)
{
    jElement.removeClass(cssClass);
    return true;
}

