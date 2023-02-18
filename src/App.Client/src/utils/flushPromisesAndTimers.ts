import { act } from "react-dom/test-utils";

function flushPromisesAndTimers() {
  return act(
    () =>
      new Promise(resolve => {
        setTimeout(resolve, 100);
        jest.runAllTimers();
      }),
  );
}

export default flushPromisesAndTimers;
