//manager.js
//mark.lawrence

(function () {
    var controllerId = 'PeopleController';

    angular.module('app.people').controller(controllerId, ['$log', 'peopleService', '$uibModal', mainController]);

    function mainController(log, service, $modal) {
        var vm = this;
        vm.title = "People";

        vm.addItem = addItem;
        vm.editItem = editItem;
        vm.deleteItem = deleteItem;

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
        }

        function addItem() {
            var item = {};
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                controller: ['$uibModalInstance', 'peopleService', 'item', EditPersonController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            });
        }

        function editItem(item) {
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                controller: ['$uibModalInstance', 'peopleService', 'item', EditPersonController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            });
        }

        function deleteItem(person) {
            service.remove(person.id).then(function (data) {
                var idx = vm.people.indexOf(person);
                vm.people.splice(idx, 1);
            });
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

    function EditPersonController($modal, service, item) {
        var vm = this;

        vm.item = angular.copy(item);

        vm.close = close;
        vm.save = save;

        function close() {
            $modal.close();
        }

        function save() {
            service.update(vm.item).then(function () {
                vm.item.updateStatus = 2;
                angular.extend(item, vm.item);
                $modal.close();
            });
        }
    }
})()