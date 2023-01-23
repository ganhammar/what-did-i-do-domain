export default function debounce<Arguments extends any[], Response = void>(
  callback: (args?: Arguments) => Response,
  delay: number
): [(args?: Arguments) => Promise<Response>, () => void] {
  let timer: NodeJS.Timer;

  const debouncedFunction = (args?: Arguments): Promise<Response> => new Promise((resolve) => {
    clearTimeout(timer);
    timer = setTimeout(() => resolve(callback(args)), delay);
  });

  const teardown = () => clearTimeout(timer);

  return [debouncedFunction, teardown];
}
