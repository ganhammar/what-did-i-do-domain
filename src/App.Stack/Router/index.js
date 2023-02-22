'use strict';

exports.handler = async (event, context, callback) => {
  const request = event.Records[0].cf.request;
  console.log('REQUEST INIT', request);
  const newBucketOrigin = 'what-did-i-do-stack-logindde9992a-s5qwsfxw77e5.s3.eu-north-1.amazonaws.com';
  const MATCHING_PATHS = ['/login/'];

  if (MATCHING_PATHS.some(path => request.uri.startsWith(path))) {
    console.log('ITS A MATCH!');
    request.origin = {
      custom: {
        domainName: newBucketOrigin,
        port: 80,
        protocol: 'http',
        path: '',
        sslProtocols: ['TLSv1', 'TLSv1.1', 'TLSv1.2'],
        readTimeout: 5,
        keepaliveTimeout: 5,
        customHeaders: { ...request.origin.custom.customHeaders }
      }
    }
    request.headers['host'] = [{ key: 'host', value: newBucketOrigin }];
  }
  callback(null, request);
};
