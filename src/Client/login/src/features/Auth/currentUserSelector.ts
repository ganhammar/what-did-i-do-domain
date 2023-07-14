import { selector } from 'recoil';
import { UserService } from '../User/UserService';

const currentUserSelector = selector({
  key: 'CurrentUser',
  get: async () => {
    const userService = new UserService();

    const response = await userService.user();

    return response.result;
  },
});

export default currentUserSelector;
