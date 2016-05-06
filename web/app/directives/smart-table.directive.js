//mark.lawrence
//smart-table.clearKey.js

(function () {
    angular.module('app').directive("stClearKey", function () {
        return {
            restrict: 'EA',
            require: '^stTable',
            link: function (scope, element, attrs, ctrl) {
                element.on('keydown', function (event) {
                    if (event.which !== 27) { return; } // check key how you want
                    return scope.$apply(function () {
                        var fieldName = element.context.attributes['st-search'].value;
                        var tableState = ctrl.tableState();
                        tableState.search.predicateObject[fieldName] = '';
                        return ctrl.pipe();
                    });
                });
            }
        };
    });
})();