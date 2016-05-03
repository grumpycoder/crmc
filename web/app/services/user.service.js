//user.service.js
//mark.lawrence

(function () {
    var serviceId = 'userService';
    angular.module('app.service').factory(serviceId, ['$log', '$http', serviceController]);

    function serviceController(log, $http) {
        log.info('loaded ' + serviceId);
        var url = 'http://localhost:11277/api/users/';

        var service = {
            availableRoles: availableRoles,
            create: create,
            get: get,
            query: query,
            remove: remove,
            update: update
        }

        return service;

        function availableRoles() {
            return $http.get(url + 'roles')
                .then(function (response) {
                    return response.data;
                });
        }

        function create(user) {
            //return $http.post(url, user)
            //   .then(function (response) {
            //       return response.data;
            //   }).catch(function (error) {
            //       return error;
            //   }
            //   );
            return $http.post(url, user)
                .then(function (response) {
                    return response.data;
                });
        }

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function query(searchTerm) {
            return $http.get(url + '?search=' + searchTerm)
                .then(function (response) {
                    return response.data;
                });
        }

        function update(user) {
            return $http.post(url + 'update', user)
                .then(function (response) {
                    return response.data;
                });
        }

        function remove(id) {
            return $http.delete(url + id)
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()