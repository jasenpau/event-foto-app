/// <reference lib="webworker" />

import { UploadMessage } from '../camera.types';
import { SasUri, SasUriResponse } from '../../../services/image/image.types';
import { SasTokenHandler, uploadPhoto } from '../uploadPhoto';

const sasTokenData: SasUri = {
  baseUri: '',
  params: '',
  eventId: 0,
  expiresOn: new Date(0),
};

addEventListener('message', async ({ data }) => {
  const { filename, eventId, captureDate, authToken, apiBaseUrl } = data;
  const root = await navigator.storage.getDirectory();

  const fileCallback = async (filename: string) => {
    const fileHandle = await root.getFileHandle(filename);
    return await fileHandle.getFile();
  };

  const sasTokenHandler: SasTokenHandler = {
    getToken: async (authToken, eventId) => {
      if (
        sasTokenData.baseUri &&
        sasTokenData.params &&
        sasTokenData.expiresOn > new Date() &&
        eventId === sasTokenData.eventId
      ) {
        return sasTokenData;
      }

      const response = await fetch(`${apiBaseUrl}/image/sas/${eventId}`, {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${authToken}`,
        },
      });

      if (response.ok) {
        const sasResponse = (await response.json()) as SasUriResponse;
        const parts = sasResponse.sasUri.split('?');
        sasTokenData.baseUri = parts[0];
        sasTokenData.params = parts[1];
        sasTokenData.expiresOn = new Date(sasResponse.expiresOn);
        sasTokenData.eventId = sasResponse.eventId;
        return sasTokenData;
      }

      throw Error('Cannot acquire SAS URI');
    },
  };

  const cleanup = async () => {
    await root.removeEntry(filename);
  };

  await uploadPhoto({
    filenames: [filename],
    eventId,
    captureDate,
    authToken,
    apiBaseUrl,
    fileCallback,
    sasTokenHandler,
    messageCallback,
    cleanup,
  });
});

const messageCallback = (message: UploadMessage) => {
  postMessage(message);
};
