//censor.service.js
//mark.lawrence

(function () {
    'use strict';

    var serviceId = 'censorService';
    angular.module('app.service').factory(serviceId, serviceController);

    function serviceController(logger, $http, config) {
        logger.log(serviceId + ' loaded');
        var url = config.apiUrl + config.apiEndPoints.Censor;

        var service = {
            create: create,
            get: get,
            query: query,
            update: update,
            remove: remove
        };

        return service;

        function create(censor) {
            return $http.post(url, censor).then(_success);
        };

        function get() {
            return $http.get(url).then(_success);
        };

        function query(searchTerm) {
            if (searchTerm == undefined) {
                return get();
            } else {
                return $http.get(url + '/?search=' + searchTerm).then(_success);
            }
        };

        function update(censor) {
            return $http.put(url, censor).then(_success);
        };

        function remove(id) {
            return $http.delete(url + '/' + id).then(_success);
        };

        function _success(response) {
            return response.data;
        };
    }
})();