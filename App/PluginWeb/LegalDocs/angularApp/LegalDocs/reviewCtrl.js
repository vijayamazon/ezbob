legalDocsControllers.controller("reviewCtrl", ['$scope', '$http', function ($scope, $http) {
    $(document).ready(function () {
        
        $(".LegalDocsTab").click(function (e) {
            var id = $(e.target).parent().attr("id");
            if (id === "ReviewLegalDocsTab") {
                $("#ReviewLegalDocsTab").addClass("active");
                $("#ApproveLegalDocsTab").removeClass("active");
            }
            if (id === "ApproveLegalDocsTab") {
                $("#ApproveLegalDocsTab").addClass("active");
                $("#ReviewLegalDocsTab").removeClass("active");
            }
            $('[data-toggle="tooltip"]').tooltip();
        });

        $('[data-toggle="popover"]').popover();

        tinymce.init({
            selector: 'textarea',
            plugins: "table"
        });
    });

    $scope.loanAgreementTemplatesRepository = null;
    $scope.currentTemplateId = 0;

    if ($scope.loanAgreementTemplatesRepository == null) {
        $http.get(urlGetLatestLegalDocs)
            .then(function (response) {
                $scope.loanAgreementTemplatesRepository = response.data.Data;
            });
    }
    $scope.OnRepeatDone = function () {
        $('.table td').click(function (event) {

            if ($(event.target).hasClass("LegalDocsIcon")) {
                $('.table tr').removeClass("highlight");
                $(event.target).closest('tr').addClass('highlight');
            }
        });
    }

    $scope.GetDownloadURl = function () {

        return urlDownloadUrl;
    }

    $scope.GetTemplateStatus = function (id) {
        var element = $.grep($scope.loanAgreementTemplatesRepository, function (t) { return t.Id === id; })[0];
        if (element.IsReviewed === true) {
            if (element.IsApproved === true)
                return "original";
            else
                return "changed";
        }
        return "changed";
    }

    $scope.GetOrigin = function (originId) {
        switch (originId) {
            case 1:
                return "Ezbob";
            case 2:
                return "Everline";
            case 3:
                return "Alibaba";
        }
        return 0;
    }

    $scope.GetTemplateName = function (templateId) {
        switch (templateId) {
            case 1:
                return "Guaranty Agreement";
            case 2:
                return "Pre Contract";
            case 3:
                return "Regulated Loan Agreement";
            case 4:
                return "Private Company Loan Agreement";
            case 5:
                return "Credit Facility";
        }
        return 0;
    }

    $scope.GetProductName = function (productId) {
        switch (productId) {
            case 1:
                return "Loans";
            case 2:
                return "Alibaba";
            case 3:
                return "CreditLine";
            case 4:
                return "Invoice Finance";
        }
        return 0;
    }

    $scope.GetBoolString = function (state) {
        switch (state) {
            case true:
                return "Yes";
            case false:
                return "No";
        }
        return "";
    }

    $scope.GetDate = function (date) {
        return moment(date).format("DD/MM/YYYY");
    }

    $scope.GetRepository = function () {
        return $scope.loanAgreementTemplatesRepository;
    }

    $scope.OnSave = function (event) {
        var $btn = $(event.target);
        $btn.text('loading...');

        var template = tinyMCE.activeEditor.getContent();
        var clone = $.grep($scope.loanAgreementTemplatesRepository, function (t) { // just use arrGetDate
            return t.Id === $scope.currentTemplateId;
        })[0];

        clone.Template = template;

        $http.post(urlSaveLegalDoc, JSON.stringify(clone))
            .then(function (response) {
                if (response.data.Success === "True") {
                    var elementId = $scope.loanAgreementTemplatesRepository.map(function (x) { return x.Id; }).indexOf($scope.currentTemplateId);
                    $scope.loanAgreementTemplatesRepository[elementId].Template = template;
                    $btn.text('Save');
                }
            });
    }

    $scope.OnAddDraft = function () {
        
        var template = tinyMCE.activeEditor.getContent();
        var clone = $.grep($scope.loanAgreementTemplatesRepository, function (t) { // just use arr
            return t.Id === $scope.currentTemplateId;
        })[0];

        var copiedObject = $.extend(true, {}, clone);

        copiedObject.Template = template;

        $http.post(urlAddLegalDoc, JSON.stringify(copiedObject))
            .then(function (response) {
                if (response.data.Success === "True")
                    $scope.loanAgreementTemplatesRepository.push(response.data.Data);
            });
    }

    $scope.OnReview = function (clicked) {

        var id = parseInt($(clicked.target).attr('data-id'));
        $http.post(urlReviewLegalDoc, { loanAgreementTemplateId: id })
            .then(function (response) {
                if (response.data.Success === "True") {

                    var elementId = $scope.loanAgreementTemplatesRepository.map(function (x) { return x.Id; }).indexOf(id);
                    $scope.loanAgreementTemplatesRepository[elementId].IsReviewed = true;
                }
            });
    }

    $scope.OnDownload = function (clicked) {

        clicked.preventDefault();

        var id = parseInt($(clicked.target).attr('data-id'));
        var selected = $.grep($scope.loanAgreementTemplatesRepository, function (t) { return t.Id === id; });
        var template = selected[0].Template;

        $http({
            method: 'POST',
            data: { template: template },
            url: urlUpdateCurrentTemplate,
        })
            .success(function (response) {

                window.open($(clicked.target).attr("href"));
            });
    }

    $scope.OnDelete = function (clicked) {

        var id = parseInt($(clicked.target).attr('data-id'));
        $http.post(urlDeleteLegalDoc, { loanAgreementTemplateId: id })
            .then(function (response) {
                if (response.data.Success === "True") {

                    var elementId = $scope.loanAgreementTemplatesRepository.map(function (x) { return x.Id; }).indexOf(id);
                    var result = $scope.loanAgreementTemplatesRepository.splice(elementId, 1);
                }
            });
    }

    $scope.OnEdit = function (clicked) {
        var id = parseInt($(clicked.target).attr('data-id'));

        if ($scope.GetTemplateStatus(id) === "original") {
            $(".AddTemplate").removeClass("disabled");
            $(".SaveTemplate").addClass("disabled");

        } else {
            $(".AddTemplate").addClass("disabled");
            $(".SaveTemplate").removeClass("disabled");

        }

        $scope.currentTemplateId = id;
        var selected = $.grep($scope.loanAgreementTemplatesRepository, function (t) { return t.Id === id; });

        var template = selected[0].Template;

        tinyMCE.activeEditor.setContent(template);
        $(tinyMCE.activeEditor.getBody()).animate({ scrollTop: 0 }, { duration: 'medium', easing: 'swing' });
    }
}]);
