//manager.js
//mark.lawrence

(function () {
    var controllerId = 'PeopleController';

    angular.module('app.people').controller(controllerId, ['$log', 'peopleService', mainController]);

    function mainController(log, service) {
        var vm = this;
        vm.title = "People";

        vm.people = [];
        vm.search = search;
        vm.searchTerm = '';

        vm.searchModel = {
            lastname: 'wong',
            page: 2
        };

        activate();

        function activate() {
            search();
        }

        vm.searchModel = {}

        function search() {
            service.query(vm.searchModel).then(function (data) {
                vm.people = data.items;
            });
        }
    }
})()