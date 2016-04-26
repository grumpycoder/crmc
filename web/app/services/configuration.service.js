//configuration.service.js
//mark.lawrence

(function () {
    var serviceId = 'configurationService';
    angular.module('app.service').factory(serviceId, ['$log', '$http', serviceController]);

    function serviceController(log, $http) {
        log.info('loaded ' + serviceId);
        var url = 'http://localhost:11277/api/configuration/';

        var service = {
            get: get,
            update: update
        }

        return service;

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function update(config) {
            return $http.put(url, config)
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()