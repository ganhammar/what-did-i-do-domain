'use strict';

exports.handler = async (event, context, callback) => {
  const request = event.Records[0].cf.request;
  console.log('REQUEST INIT', { uri: request.uri, target: request.origin.s3.domainName });
  const MATCHING_EXTENSIONS = ['.js', '.css', '.json', '.txt', '.html', '.map'];

  if (!MATCHING_EXTENSIONS.some(path => request.uri.endsWith(path))) {
    console.log('REDIRECT TO index.html');
    request.uri = request.uri.startsWith('/login') ? '/login/index.html' : '/index.html ';
  }

  callback(null, request);
};
