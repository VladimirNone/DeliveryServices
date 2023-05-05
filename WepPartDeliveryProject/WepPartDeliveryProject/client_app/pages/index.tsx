import Head from 'next/head'
import { FC, useState } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticProps } from 'next'
import DishMainCard from '@/components/cards/DishMainCard'

export const getStaticProps:GetStaticProps = async () => {
  const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
  const data = await resp.json() as categoryItem[];

  const resp2 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getDishesForMainPage`);
  const dishListInfo = await resp2.json() as {dishes: dishClientInfo[], pageEnded: boolean};

  return {
    props:{
      categories: data,
      dishesProps: dishListInfo.dishes,
      pageEndedProps: dishListInfo.pageEnded,
    }
  }
}

type homePageProps = {
  categories:categoryItem[], 
  dishesProps: dishClientInfo[],
  pageEndedProps: boolean,
}

const Home: FC<homePageProps> = ({categories, dishesProps, pageEndedProps}) => {
  const [dishesState, setDishesState] = useState<dishClientInfo[]>(dishesProps);
  //нулевая страница загружается при переходе на страницу
  const [page, setPage] = useState(1);
  const [pageEnded, setPageEnded] = useState(pageEndedProps);

  const handleShowMoreDishes = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getDishesForMainPage?page=${page}`);
    const loadedData = await resp.json() as {dishes: dishClientInfo[], pageEnded: boolean};

    if(resp.ok){
        setPage(page + 1);
        setDishesState(dishesState.concat(loadedData.dishes));
        setPageEnded(loadedData.pageEnded);
    }
    else{
      setPageEnded(true);
    }
  }

  return (
    <ClientLayout categories={categories}>
      <Head>
        <title>Create Next App</title>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main>
        <div>
          {dishesState.map((dish, i) => <DishMainCard key={i} {...dish}/>)}
        </div>
        {!pageEnded && (<div>
          <button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreDishes}>
            Показать больше
          </button>
        </div>)
        }
      </main>
    </ClientLayout>
  )
}

export default Home;