'use strict';

exports.handler = async (event, context, callback) => {
  const request = event.Records[0].cf.request;
  console.log('REQUEST INIT', { uri: request.uri, target: request.origin.s3.domainName });
  const MATCHING_EXTENSIONS = ['.js', '.css', '.json', '.txt', '.html', '.map', '.png', '.jpg', '.svg'];

  if (!MATCHING_EXTENSIONS.some(path => request.uri.endsWith(path))) {
    let prefix = '';

    if (request.uri.startsWith('/login')) {
      prefix = '/login';
    }

    request.uri = `${prefix}/index.html`;
  }

  console.log('REQUEST END', { uri: request.uri });
  callback(null, request);
};
