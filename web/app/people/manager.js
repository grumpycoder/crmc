//manager.js
//mark.lawrence

(function () {
    var controllerId = 'PeopleController';

    angular.module('app.people').controller(controllerId, ['$log', 'peopleService', mainController]);

    function mainController(log, service) {
        var vm = this;
        vm.title = "People";

        vm.people = [];
        vm.paged = paged;
        vm.search = search;
        vm.searchTerm = '';

        vm.searchModel = {
            page: 1,
            pageSize: 15
        };
        var tableStateRef;

        activate();

        function activate() {
            //search();
            //tableState.sort.predicate = undefined;
        }

        function search(tableState) {
            tableStateRef = tableState;

            if (typeof (tableState.sort.predicate) != "undefined") {
                vm.searchModel.orderBy = tableState.sort.predicate;
                vm.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }
            if (typeof (tableState.search.predicateObject) != "undefined") {
                vm.searchModel.firstname = tableState.search.predicateObject.firstname;
                vm.searchModel.lastname = tableState.search.predicateObject.lastname;
                vm.searchModel.zipcode = tableState.search.predicateObject.zipCode;
                vm.searchModel.emailAddress = tableState.search.predicateObject.emailAddress;
                vm.searchModel.isDonor = tableState.search.predicateObject.isDonor;
                vm.searchModel.isPriority = tableState.search.predicateObject.isPriority;
                vm.searchModel.dateCreated = tableState.search.predicateObject.dateCreated;
                vm.searchModel.fuzzyMatchValue = tableState.search.predicateObject.fuzzyMatchValue;
            }

            service.query(vm.searchModel).then(function (data) {
                vm.people = data.items;
                vm.searchModel = data;
                //log.info(vm.searchModel);
            });
        }

        function paged(pageNum) {
            log.info(pageNum);
            search(tableStateRef);
        }
    }
})()