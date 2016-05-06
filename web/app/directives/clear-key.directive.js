//mark.lawrence
//clear-key.directive.js

(function () {
    angular.module('app').directive("clearKey", function () {
        return {
            restrict: 'EA',
            require: 'ngModel',
            link: function (scope, el, attrs, ctrl) {
                el.on('keydown', function (event) {
                    if (event.which !== 27) { return; } // check key how you want
                    ctrl.$setViewValue(null);
                    ctrl.$render();
                    scope.$apply();
                });
            }
        };
    });
})();