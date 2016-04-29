(function () {
    'use strict';

    var controllerId = 'HomeController';

    angular.module('app.home').controller(controllerId, ['$log', 'peopleService', HomeController]);

    function HomeController(log, service) {
        var vm = this;
        vm.title = 'Home';

        vm.stat = {};
        vm.people = [];

        activate();

        function activate() {
            log.info(controllerId + ' active');
            refresh();
        }

        function refresh() {
            service.getCurrentStats()
                .then(function (data) {
                    vm.stat = data;
                });

            service.get()
                .then(function (data) {
                    vm.people = data;
                });
        }
    }
})();