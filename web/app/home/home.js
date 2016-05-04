(function () {
    'use strict';

    var controllerId = 'HomeController';

    angular.module('app.home').controller(controllerId, ['$log', 'peopleService', 'censorService', 'storage', mainController]);

    function mainController(log, peopleService, censorService, storage) {
        var vm = this;
        vm.title = 'Home';

        vm.stat = {};
        vm.people = [];

        activate();

        function activate() {
            log.info(controllerId + ' active');

            censorService.query('')
                    .then(function (data) {
                        storage.set('censors', JSON.stringify(data));
                    });
            refresh();
        }

        function refresh() {
            peopleService.getCurrentStats()
                .then(function (data) {
                    vm.stat = data;
                });

            peopleService.get()
                .then(function (data) {
                    vm.people = data;
                });
        }
    }
})();