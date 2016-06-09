//user.service.js
//mark.lawrence

(function () {
    'use strict';

    var serviceId = 'userService';
    angular.module('app.service').factory(serviceId, serviceController);

    function serviceController(logger, $http, config) {
        logger.log(serviceId + ' loaded');
        var url = config.apiUrl + config.apiEndPoints.User;

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
            return $http.get(url + '/roles').then(_success);
        }

        function create(user) {
            return $http.post(url, user).then(_success, function (error) {
                logger.log('error', error);
            });
        }

        //TODO: Need a getbyname function
        function get() {
            return $http.get(url).then(_success);
        }

        function query(searchTerm) {
            url += '/search/';

            if (searchTerm != undefined && searchTerm.length > 0) {
                url += '?' + searchTerm;
            };
            return $http.get(url).then(_success);
        }

        function update(user) {
            return $http.put(url, user).then(_success);
        }

        function remove(id) {
            return $http.delete(url + '/' + id).then(_success);
        }

        function _success(response) {
            return response.data;
        }
    }
})();