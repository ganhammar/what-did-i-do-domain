import { act } from "react-dom/test-utils";

export function flushPromisesAndTimers() {
  return act(
    () =>
      new Promise(resolve => {
        setTimeout(resolve, 100);
        jest.runAllTimers();
      }),
  );
}
