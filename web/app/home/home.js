(function () {
    'use strict';

    var controllerId = 'HomeController';

    angular.module('app.home').controller(controllerId, ['$log', HomeController]);

    function HomeController(log) {
        var vm = this;
        vm.title = 'Home';

        activate();

        function activate() {
            log.info(controllerId + ' active');
        }
    }
})();