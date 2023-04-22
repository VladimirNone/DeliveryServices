import { FC, useState } from "react"
import ClientLayout from '@/components/ClientLayout'
import { GetStaticPaths, GetStaticProps } from 'next'
import { Carousel, Row, Image, Col } from 'react-bootstrap'
import Head from 'next/head'
import styles from '@/styles/Home.module.css'
import CountDishToCart from "@/components/CountDishToCart"

export const getStaticPaths: GetStaticPaths = async () => {
  const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getDishIds`);
  const dishIds = await resp.json() as string[];
  const paths = dishIds.map((value) => ({ params: { id: value } }))

  return {
    paths,
    fallback: false, // can also be true or 'blocking'
  }
}

export const getStaticProps: GetStaticProps = async (context) => {
  const id = context.params?.id;
  const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
  const categoryList = await resp1.json() as categoryItem[];

  const resp2 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getDish/${id}`);
  const dish = await resp2.json() as dishClientCardProps;

  return {
    props: {
      categories: categoryList,
      dish,
    }
  }
}

type dishPageProps = {
  categories: categoryItem[],
  dish: dishClientCardProps,
}

const Dish: FC<dishPageProps> = ({ categories, dish }) => {
  const [index, setIndex] = useState(0);

  const handleSelect = (selectedIndex: number):void => {
    setIndex(selectedIndex);
  };

  return (
    <ClientLayout categories={categories}>
      <Head>
        <title>Create Next App</title>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main>
        <div>
          <Row className="justify-content-center">
            <Col xs={12} sm={10} md={8} lg={6}>
              <Carousel activeIndex={index} onSelect={handleSelect}>
                {dish.images?.map((value, i) =>
                  <Carousel.Item key={i} >
                    <Image className="d-block w-100" src={value} alt="No image" />
                  </Carousel.Item>
                )}
              </Carousel>
            </Col>
          </Row>
          <Row className="justify-content-center mb-3">
            <Col xs={12} sm={11} md={10} lg={9}>
              <div>
                <div className="d-flex mb-3 mt-2">
                  <h5 className="flex-grow-1">{dish.name}</h5>
                  <h5 className=""><b>{dish.price}Р</b></h5>
                </div>
                <p className="text-justify">{dish.description}</p>
              </div>
              <CountDishToCart dishId={dish.id}/>
            </Col>
          </Row>
        </div>
      </main>
    </ClientLayout>
  )
}

export default Dish;