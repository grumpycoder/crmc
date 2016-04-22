//censor.service.js
//mark.lawrence

(function () {
    var serviceId = 'censorService';
    angular.module('app.service').factory(serviceId, ['$log', '$http', serviceController]);

    function serviceController(log, $http) {
        log.info('loaded ' + serviceId);
        var service = {
        }
    }
})()