import Head from 'next/head'
import { FC } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticPaths, GetStaticProps } from 'next'
import DishMainCard from '@/components/cards/DishMainCard'

export const getStaticPaths:GetStaticPaths = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];
    const paths = categories.map((value)=> ({params: {category: value.linkName}}))

  return {
    paths,
    fallback: false, // can also be true or 'blocking'
  }
}

export const getStaticProps:GetStaticProps = async (context) => {
    const category = context.params?.category;
    const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categoryList = await resp1.json() as categoryItem[];

    const resp2 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getDishesList/${category}`);
    const dishList = await resp2.json() as dishClientInfo[];

    return {
        props:{
            categories: categoryList,
            dishes: dishList,
        }
    }
}

type categoryPageProps = {
  categories:categoryItem[], 
  dishes: dishClientInfo[],
}

const Category: FC<categoryPageProps> = ({categories, dishes}) => {
  return (
    <ClientLayout categories={categories}>
      <Head>
        <title>Create Next App</title>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main>
        <div>
          {dishes.map((dish, i) => <DishMainCard key={i} {...dish}/>)}
        </div>
      </main>
    </ClientLayout>
  )
}

export default Category;