export type LocationData = {
    id: number;
    image?: string;
    editorID: string;
    formKey: string;
    name: string;
    parentLocation: string;
    region: string;
    keywords: string[];
    locationCategory: string;
    locationType: string;
    status: string;
    relatedQuestName: string;
    relatedQuestUrl: string;
    inhabitants: string;
    // questLinks: string[];
    // vikunjaLink: string[];
    notes: string;
    // wishlist: string;
    // lastEdited: string;
    // editedBy: string;
};

export type LocationFilters = {
  statuses: string[];
  keywords: string[];
  locationCategories : string[];
  locationTypes: string[];
  parentLocationsCities: string[];
  parentLocations: string[];
  inhabitants: string[];
};