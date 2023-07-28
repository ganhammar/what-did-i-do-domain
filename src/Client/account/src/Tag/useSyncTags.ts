import { useRecoilState, useRecoilValue } from 'recoil';
import { tagsAtom } from './tagsAtom';
import { ApiResponse } from '@wdid/shared/src/infrastructure/FetchBase';
import { currentAccountAtom } from 'src/Account';

export const useSyncTags = () => {
  const account = useRecoilValue(currentAccountAtom);
  const [tags, setTags] = useRecoilState(tagsAtom);

  return (tagsToSync: string[]) => {
    console.log(tags);
    const updatedTags: ApiResponse<Tag[]> = {
      ...tags,
      result: [...(tags.result ?? [])],
    };

    tagsToSync.forEach((tag) => {
      if (updatedTags.result!.some(({ value }) => value === tag) === false) {
        updatedTags.result!.push({
          accountId: account.id,
          value: tag,
        });
      }
    });

    setTags(updatedTags);
  };
};
