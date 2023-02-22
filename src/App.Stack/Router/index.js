'use strict';

exports.handler = async (event, context, callback) => {
  const request = event.Records[0].cf.request;
  console.log('REQUEST INIT', request);
  const MATCHING_EXTENSIONS = ['.js', '.css', '.json', '.txt', '.html', '.map'];

  if (!MATCHING_EXTENSIONS.some(path => request.uri.startsWith(path))) {
    console.log('REDIRECT TO index.html');
    request.uri = '/index.html';
  }

  callback(null, request);
};
