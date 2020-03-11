import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public shoppingItems: ShoppingItem[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<ShoppingItem[]>(baseUrl + 'api/Shopping/Items').subscribe(result => {
      this.shoppingItems = result;
    }, error => console.error(error));
  }
}

interface ShoppingItem {
  ingredient: Ingredient;
  recipeNames: string[];
}

interface Ingredient {
  name: string;
  quantity: number;
  quantityType: string;
}


