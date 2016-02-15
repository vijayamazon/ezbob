var legalDocsApp = angular.module('legalDocsApp', ['ngRoute', 'legalDocsControllers']);

legalDocsApp.config(function ($routeProvider) {
    $routeProvider
        .when('/approvePage', {
            templateUrl: '/angularApp/LegalDocs/ApproveLegalDocs.html',
            controller: 'approveCtrl'
        })
    .when('/reviewPage', {
        templateUrl: '/angularApp/LegalDocs/ReviewLegalDocs.html',
        controller: 'reviewCtrl'
    });
});

legalDocsApp.directive('repeatDone', function () {
    return function (scope, element, attrs) {
        if (scope.$last) { // all are rendered
            scope.$eval(attrs.repeatDone);
        }
    }
});

var legalDocsControllers = angular.module('legalDocsControllers', []);
